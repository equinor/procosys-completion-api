using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;

public class CreatePunchItemCommandHandler : IRequestHandler<CreatePunchItemCommand, Result<GuidAndRowVersion>>
{
    private readonly IPlantProvider _plantProvider;
    private readonly IPunchItemRepository _punchItemRepository;
    private readonly ILibraryItemRepository _libraryItemRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IWorkOrderRepository _woRepository;
    private readonly ISWCRRepository _swcrRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ISyncToPCS4Service _syncToPCS4Service;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageProducer _messageProducer;
    private readonly ICheckListApiService _checkListApiService;
    private readonly ILogger<CreatePunchItemCommandHandler> _logger;

    public CreatePunchItemCommandHandler(
        IPlantProvider plantProvider,
        IPunchItemRepository punchItemRepository,
        ILibraryItemRepository libraryItemRepository,
        IProjectRepository projectRepository,
        IPersonRepository personRepository,
        IWorkOrderRepository woRepository,
        ISWCRRepository swcrRepository,
        IDocumentRepository documentRepository,
        ISyncToPCS4Service syncToPCS4Service,
        IUnitOfWork unitOfWork,
        IMessageProducer messageProducer,
        ICheckListApiService checkListApiService,
        ILogger<CreatePunchItemCommandHandler> logger)
    {
        _plantProvider = plantProvider;
        _punchItemRepository = punchItemRepository;
        _libraryItemRepository = libraryItemRepository;
        _projectRepository = projectRepository;
        _personRepository = personRepository;
        _woRepository = woRepository;
        _swcrRepository = swcrRepository;
        _documentRepository = documentRepository;
        _syncToPCS4Service = syncToPCS4Service;
        _unitOfWork = unitOfWork;
        _messageProducer = messageProducer;
        _checkListApiService = checkListApiService;
        _logger = logger;
    }

    public async Task<Result<GuidAndRowVersion>> Handle(CreatePunchItemCommand request,
        CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetAsync(request.ProjectGuid, cancellationToken);

        var raisedByOrg = await _libraryItemRepository.GetByGuidAndTypeAsync(
            request.RaisedByOrgGuid,
            LibraryType.COMPLETION_ORGANIZATION,
            cancellationToken);
        var clearingByOrg = await _libraryItemRepository.GetByGuidAndTypeAsync(
            request.ClearingByOrgGuid,
            LibraryType.COMPLETION_ORGANIZATION,
            cancellationToken);

        var punchItem = new PunchItem(
            _plantProvider.Plant,
            project,
            request.CheckListGuid,
            request.Category,
            request.Description,
            raisedByOrg,
            clearingByOrg);

        var properties = GetRequiredProperties(punchItem);

        await SetActionByAsync(punchItem, request.ActionByPersonOid, properties, cancellationToken);
        SetDueTime(punchItem, request.DueTimeUtc, properties);
        await SetLibraryItemAsync(punchItem, request.PriorityGuid, LibraryType.PUNCHLIST_PRIORITY, properties,
            cancellationToken);
        await SetLibraryItemAsync(punchItem, request.SortingGuid, LibraryType.PUNCHLIST_SORTING, properties,
            cancellationToken);
        await SetLibraryItemAsync(punchItem, request.TypeGuid, LibraryType.PUNCHLIST_TYPE, properties,
            cancellationToken);
        SetEstimate(punchItem, request.Estimate, properties);
        await SetOriginalWorkOrderAsync(punchItem, request.OriginalWorkOrderGuid, properties, cancellationToken);
        await SetWorkOrderAsync(punchItem, request.WorkOrderGuid, properties, cancellationToken);
        await SetSWCRAsync(punchItem, request.SWCRGuid, properties, cancellationToken);
        await SetDocumentAsync(punchItem, request.DocumentGuid, properties, cancellationToken);
        SetExternalItemNo(punchItem, request.ExternalItemNo, properties);
        SetMaterialRequired(punchItem, request.MaterialRequired, properties);
        SetMaterialETAUtc(punchItem, request.MaterialETAUtc, properties);
        SetMaterialExternalNo(punchItem, request.MaterialExternalNo, properties);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            _punchItemRepository.Add(punchItem);

            // must save twice when creating. Must save before publishing events both to set with internal database ID
            // since ItemNo depend on it. Must save after publishing events because we use outbox pattern
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Add property for ItemNo first in list, since it is an "important" property
            properties.Insert(0, new Property(nameof(PunchItem.ItemNo), punchItem.ItemNo, ValueDisplayType.IntAsText));

            var integrationEvent =
                await PublishPunchItemCreatedIntegrationEventsAsync(punchItem, properties, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _syncToPCS4Service.SyncNewPunchListItemAsync(integrationEvent, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            await _checkListApiService.RecalculateCheckListStatus(punchItem.CheckListGuid, cancellationToken);

            _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} created", punchItem.ItemNo,
                punchItem.Guid);

            return new SuccessResult<GuidAndRowVersion>(new GuidAndRowVersion(punchItem.Guid,
                punchItem.RowVersion.ConvertToString()));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred on insertion of PunchListItem");
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<PunchItemCreatedIntegrationEvent> PublishPunchItemCreatedIntegrationEventsAsync(
        PunchItem punchItem,
        List<IProperty> properties,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new PunchItemCreatedIntegrationEvent(punchItem);
        await _messageProducer.PublishAsync(integrationEvent, cancellationToken);

        var historyEvent = new HistoryCreatedIntegrationEvent(
            $"Punch item {punchItem.Category} {punchItem.ItemNo} created",
            punchItem.Guid,
            punchItem.CheckListGuid,
            new User(punchItem.CreatedBy.Guid, punchItem.CreatedBy.GetFullName()),
            punchItem.CreatedAtUtc,
            properties);
        await _messageProducer.SendHistoryAsync(historyEvent, cancellationToken);

        return integrationEvent;
    }

    private List<IProperty> GetRequiredProperties(PunchItem punchItem)
        =>
        [
            new Property(nameof(PunchItem.Category), punchItem.Category.ToString()),
            new Property(nameof(PunchItem.Description), punchItem.Description),
            new Property(nameof(PunchItem.RaisedByOrg), punchItem.RaisedByOrg.ToString()),
            new Property(nameof(PunchItem.ClearingByOrg), punchItem.ClearingByOrg.ToString())
        ];

    private void SetMaterialExternalNo(PunchItem punchItem, string? materialExternalNo, List<IProperty> properties)
    {
        if (materialExternalNo is null)
        {
            return;
        }

        punchItem.MaterialExternalNo = materialExternalNo;
        properties.Add(new Property(nameof(PunchItem.MaterialExternalNo), punchItem.MaterialExternalNo));
    }

    private void SetMaterialETAUtc(PunchItem punchItem, DateTime? materialETAUtc, List<IProperty> properties)
    {
        if (materialETAUtc is null)
        {
            return;
        }

        punchItem.MaterialETAUtc = materialETAUtc;
        properties.Add(new Property(nameof(PunchItem.MaterialETAUtc), punchItem.MaterialETAUtc,
            ValueDisplayType.DateTimeAsDateOnly));
    }

    private void SetMaterialRequired(PunchItem punchItem, bool materialRequired, List<IProperty> properties)
    {
        if (!materialRequired)
        {
            return;
        }

        punchItem.MaterialRequired = materialRequired;
        properties.Add(new Property(nameof(PunchItem.MaterialRequired), punchItem.MaterialRequired,
            ValueDisplayType.BoolAsYesNo));
    }

    private void SetExternalItemNo(PunchItem punchItem, string? externalItemNo, List<IProperty> properties)
    {
        if (externalItemNo is null)
        {
            return;
        }

        punchItem.ExternalItemNo = externalItemNo;
        properties.Add(new Property(nameof(PunchItem.ExternalItemNo), punchItem.ExternalItemNo));
    }

    private void SetEstimate(PunchItem punchItem, int? estimate, List<IProperty> properties)
    {
        punchItem.Estimate = estimate;
        if (estimate is null)
        {
            return;
        }

        properties.Add(new Property(nameof(PunchItem.Estimate), estimate.Value, ValueDisplayType.IntAsText));
    }

    private void SetDueTime(PunchItem punchItem, DateTime? dueTimeUtc, List<IProperty> properties)
    {
        if (dueTimeUtc is null)
        {
            return;
        }

        punchItem.DueTimeUtc = dueTimeUtc;
        properties.Add(new Property(nameof(PunchItem.DueTimeUtc), punchItem.DueTimeUtc.Value,
            ValueDisplayType.DateTimeAsDateOnly));
    }

    private async Task SetDocumentAsync(
        PunchItem punchItem,
        Guid? documentGuid,
        List<IProperty> properties,
        CancellationToken cancellationToken)
    {
        if (documentGuid is null)
        {
            return;
        }

        var doc = await _documentRepository.GetAsync(documentGuid.Value, cancellationToken);
        punchItem.SetDocument(doc);
        properties.Add(new Property(nameof(PunchItem.Document), punchItem.Document!.No));
    }

    private async Task SetSWCRAsync(
        PunchItem punchItem,
        Guid? swcrGuid,
        List<IProperty> properties,
        CancellationToken cancellationToken)
    {
        if (swcrGuid is null)
        {
            return;
        }

        var swcr = await _swcrRepository.GetAsync(swcrGuid.Value, cancellationToken);
        punchItem.SetSWCR(swcr);
        properties.Add(new Property(nameof(PunchItem.SWCR), punchItem.SWCR!.No, ValueDisplayType.IntAsText));
    }

    private async Task SetOriginalWorkOrderAsync(
        PunchItem punchItem,
        Guid? originalWorkOrderGuid,
        List<IProperty> properties,
        CancellationToken cancellationToken)
    {
        if (originalWorkOrderGuid is null)
        {
            return;
        }

        var wo = await _woRepository.GetAsync(originalWorkOrderGuid.Value, cancellationToken);
        punchItem.SetOriginalWorkOrder(wo);
        properties.Add(new Property(nameof(PunchItem.OriginalWorkOrder), punchItem.OriginalWorkOrder!.No));
    }

    private async Task SetWorkOrderAsync(
        PunchItem punchItem,
        Guid? workOrderGuid,
        List<IProperty> properties,
        CancellationToken cancellationToken)
    {
        if (workOrderGuid is null)
        {
            return;
        }

        var wo = await _woRepository.GetAsync(workOrderGuid.Value, cancellationToken);
        punchItem.SetWorkOrder(wo);
        properties.Add(new Property(nameof(PunchItem.WorkOrder), punchItem.WorkOrder!.No));
    }

    private async Task SetActionByAsync(
        PunchItem punchItem,
        Guid? actionByPersonOid,
        List<IProperty> properties,
        CancellationToken cancellationToken)
    {
        if (actionByPersonOid is null)
        {
            return;
        }

        var person = await _personRepository.GetOrCreateAsync(actionByPersonOid.Value, cancellationToken);
        punchItem.SetActionBy(person);
        properties.Add(new Property(
            nameof(PunchItem.ActionBy),
            new User(punchItem.ActionBy!.Guid, punchItem.ActionBy!.GetFullName()),
            ValueDisplayType.UserAsNameOnly));
    }

    private async Task SetLibraryItemAsync(
        PunchItem punchItem,
        Guid? libraryGuid,
        LibraryType libraryType,
        List<IProperty> properties,
        CancellationToken cancellationToken)
    {
        if (libraryGuid is null)
        {
            return;
        }

        var libraryItem =
            await _libraryItemRepository.GetByGuidAndTypeAsync(libraryGuid.Value, libraryType, cancellationToken);

        switch (libraryType)
        {
            case LibraryType.PUNCHLIST_PRIORITY:
                punchItem.SetPriority(libraryItem);
                properties.Add(new Property(nameof(PunchItem.Priority), punchItem.Priority!.ToString()));
                break;
            case LibraryType.PUNCHLIST_SORTING:
                punchItem.SetSorting(libraryItem);
                properties.Add(new Property(nameof(PunchItem.Sorting), punchItem.Sorting!.ToString()));
                break;
            case LibraryType.PUNCHLIST_TYPE:
                punchItem.SetType(libraryItem);
                properties.Add(new Property(nameof(PunchItem.Type), punchItem.Type!.ToString()));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(libraryType), libraryType, null);
        }
    }
}
