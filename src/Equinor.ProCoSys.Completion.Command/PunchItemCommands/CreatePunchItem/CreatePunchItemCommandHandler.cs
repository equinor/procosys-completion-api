using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.EventPublishers.HistoryEvents;
using Equinor.ProCoSys.Completion.Command.EventPublishers.PunchItemEvents;
using Equinor.ProCoSys.Completion.DbSyncToPCS4;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.MessageContracts;
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
    private readonly IPunchEventPublisher _punchEventPublisher;
    private readonly IHistoryEventPublisher _historyEventPublisher;
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
        IPunchEventPublisher punchEventPublisher,
        IHistoryEventPublisher historyEventPublisher,
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
        _punchEventPublisher = punchEventPublisher;
        _historyEventPublisher = historyEventPublisher;
        _logger = logger;
    }

    public async Task<Result<GuidAndRowVersion>> Handle(CreatePunchItemCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
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

            await SetActionByAsync(punchItem, request.ActionByPersonOid, cancellationToken);
            punchItem.DueTimeUtc = request.DueTimeUtc;
            await SetLibraryItemAsync(punchItem, request.PriorityGuid, LibraryType.PUNCHLIST_PRIORITY, cancellationToken);
            await SetLibraryItemAsync(punchItem, request.SortingGuid, LibraryType.PUNCHLIST_SORTING, cancellationToken);
            await SetLibraryItemAsync(punchItem, request.TypeGuid, LibraryType.PUNCHLIST_TYPE, cancellationToken);
            punchItem.Estimate = request.Estimate;
            await SetOriginalWorkOrderAsync(punchItem, request.OriginalWorkOrderGuid, cancellationToken);
            await SetWorkOrderAsync(punchItem, request.WorkOrderGuid, cancellationToken);
            await SetSWCRAsync(punchItem, request.SWCRGuid, cancellationToken);
            await SetDocumentAsync(punchItem, request.DocumentGuid, cancellationToken);
            punchItem.ExternalItemNo = request.ExternalItemNo;
            punchItem.MaterialRequired = request.MaterialRequired;
            punchItem.MaterialETAUtc = request.MaterialETAUtc;
            punchItem.MaterialExternalNo = request.MaterialExternalNo;

            _punchItemRepository.Add(punchItem);

            // AuditData must be set before publishing events due to use of Created- and Modified-properties
            await _unitOfWork.SetAuditDataAsync();

            var integrationEvent = await _punchEventPublisher.PublishCreatedEventAsync(punchItem, cancellationToken);

            await _historyEventPublisher.PublishCreatedEventAsync(
                punchItem.Plant,
                "Punch item created",
                punchItem.Guid,
                punchItem.CheckListGuid,
                new User(punchItem.CreatedBy.Guid, punchItem.CreatedBy.GetFullName()),
                punchItem.CreatedAtUtc,
                // todo 109354 add list of properties
                [],
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _syncToPCS4Service.SyncNewObjectAsync(SyncToPCS4Service.PunchItem, integrationEvent, punchItem.Plant, cancellationToken);

            // todo 109356 add unit tests
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} created", punchItem.ItemNo, punchItem.Guid);

            return new SuccessResult<GuidAndRowVersion>(new GuidAndRowVersion(punchItem.Guid, punchItem.RowVersion.ConvertToString()));
        }
        catch (Exception)
        {
            _logger.LogError("Error occurred on insertion of punch item.");
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task SetDocumentAsync(PunchItem punchItem, Guid? documentGuid, CancellationToken cancellationToken)
    {
        if (!documentGuid.HasValue)
        {
            return;
        }

        var doc = await _documentRepository.GetAsync(documentGuid.Value, cancellationToken);
        punchItem.SetDocument(doc);
    }

    private async Task SetSWCRAsync(PunchItem punchItem, Guid? swcrGuid, CancellationToken cancellationToken)
    {
        if (!swcrGuid.HasValue)
        {
            return;
        }

        var swcr = await _swcrRepository.GetAsync(swcrGuid.Value, cancellationToken);
        punchItem.SetSWCR(swcr);
    }

    private async Task SetOriginalWorkOrderAsync(PunchItem punchItem, Guid? originalWorkOrderGuid, CancellationToken cancellationToken)
    {
        if (!originalWorkOrderGuid.HasValue)
        {
            return;
        }

        var wo = await _woRepository.GetAsync(originalWorkOrderGuid.Value, cancellationToken);
        punchItem.SetOriginalWorkOrder(wo);
    }

    private async Task SetWorkOrderAsync(PunchItem punchItem, Guid? workOrderGuid, CancellationToken cancellationToken)
    {
        if (!workOrderGuid.HasValue)
        {
            return;
        }

        var wo = await _woRepository.GetAsync(workOrderGuid.Value, cancellationToken);
        punchItem.SetWorkOrder(wo);
    }

    private async Task SetActionByAsync(PunchItem punchItem, Guid? actionByPersonOid, CancellationToken cancellationToken)
    {
        if (!actionByPersonOid.HasValue)
        {
            return;
        }

        var person = await _personRepository.GetOrCreateAsync(actionByPersonOid.Value, cancellationToken);
        punchItem.SetActionBy(person);
    }

    private async Task SetLibraryItemAsync(
        PunchItem punchItem,
        Guid? libraryGuid,
        LibraryType libraryType,
        CancellationToken cancellationToken)
    {
        if (!libraryGuid.HasValue)
        {
            return;
        }
        var libraryItem = await _libraryItemRepository.GetByGuidAndTypeAsync(libraryGuid.Value, libraryType, cancellationToken);

        switch (libraryType)
        {
            case LibraryType.PUNCHLIST_PRIORITY:
                punchItem.SetPriority(libraryItem);
                break;
            case LibraryType.PUNCHLIST_SORTING:
                punchItem.SetSorting(libraryItem);
                break;
            case LibraryType.PUNCHLIST_TYPE:
                punchItem.SetType(libraryItem);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(libraryType), libraryType, null);
        }
    }
}
