using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.PcsServiceBus.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;


namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class PunchItemEventConsumer : IConsumer<PunchItemEvent>
{
    private readonly ILogger<PunchItemEventConsumer> _logger;
    private readonly IPlantSetter _plantSetter;
    private readonly IPunchItemRepository _punchItemRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ILibraryItemRepository _libraryItemRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ISWCRRepository _swcrRepository;
    private readonly IWorkOrderRepository _woRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserSetter _currentUserSetter;
    private readonly IOptionsMonitor<ApplicationOptions> _applicationOptions;

    public PunchItemEventConsumer(ILogger<PunchItemEventConsumer> logger,
        IPlantSetter plantSetter,
        IPunchItemRepository punchItemRepository,
        IProjectRepository projectRepository,
        ILibraryItemRepository libraryItemRepository,
        IDocumentRepository documentRepository,
        ISWCRRepository swcrRepository,
        IWorkOrderRepository woRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserSetter currentUserSetter,
        IOptionsMonitor<ApplicationOptions> applicationOptions)
    {
        _logger = logger;
        _plantSetter = plantSetter;
        _punchItemRepository = punchItemRepository;
        _projectRepository = projectRepository;
        _libraryItemRepository = libraryItemRepository;
        _documentRepository = documentRepository;
        _swcrRepository = swcrRepository;
        _woRepository = woRepository;
        _unitOfWork = unitOfWork;
        _currentUserSetter = currentUserSetter;
        _applicationOptions = applicationOptions;
    }

    public async Task Consume(ConsumeContext<PunchItemEvent> context)
    {
        var busEvent = context.Message;

        ValidateMessage(busEvent);
        _plantSetter.SetPlant(busEvent.Plant);

        if (await _punchItemRepository.ExistsAsync(busEvent.ProCoSysGuid, context.CancellationToken))
        {
            var punchItem = await _punchItemRepository.GetAsync(busEvent.ProCoSysGuid, context.CancellationToken);
       //     MapFromEventToWorkOrder(busEvent, punchItem);
        }
        else
        {
            var punchItem = await CreatePunchItem(busEvent, context.CancellationToken);
            _punchItemRepository.Add(punchItem);
        }
        
        _currentUserSetter.SetCurrentUserOid(_applicationOptions.CurrentValue.ObjectId);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Document Message Consumed: {MessageId} \n Guid {Guid} \n {No}",
            context.MessageId, busEvent.ProCoSysGuid, busEvent.PunchItemNo);
    }

    /*  private static void MapFromEventToPunchItem(IPunchListItemEventV1 punchItemEvent, PunchItem punchItem)
      {
          // punchItem.Type
          //punchItem.Description
          punchItem.CheckListGuid = punchItemEvent.ChecklistGuid;

      }*/

    private static void ValidateMessage(PunchItemEvent busEvent)
    {
        if (busEvent.ProCoSysGuid == Guid.Empty)
        {
            throw new Exception("Message is missing ProCoSysGuid");
        }

        if (string.IsNullOrEmpty(busEvent.Plant))
        {
            throw new Exception("Message is missing Plant");
        }

        if (string.IsNullOrEmpty(busEvent.Description))
        {
            throw new Exception("Message is missing Description");
        }
    }

    private async Task<PunchItem> CreatePunchItem(IPunchListItemEventV1 busEvent, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetAsync(busEvent.ProjectGuid, cancellationToken);

        // TODO Consider making fields mandatory in pcaBus
        var raisedByOrg = busEvent.RaisedByOrgGuid.HasValue ? await _libraryItemRepository.GetAsync(
            busEvent.RaisedByOrgGuid.Value,
            cancellationToken) : throw new Exception("Message is missing RaisedByOrgGuid");
        var clearingByOrg = busEvent.ClearingByOrgGuid.HasValue ? await _libraryItemRepository.GetAsync(
            busEvent.ClearingByOrgGuid.Value, cancellationToken) : throw new Exception("Message is missing ClearingByOrgGuid");

        var punchItem = new PunchItem(
            busEvent.Plant,
            project,
            busEvent.ChecklistGuid,
            Enum.Parse<Category>(busEvent.Category),
            busEvent.Description!,
            raisedByOrg,
            clearingByOrg,
            busEvent.ProCoSysGuid);
        
        // TODO not in bus message, need to resolve (nullable in db)?
        //await SetActionByAsync(punchItem, request.ActionByPersonOid, properties, cancellationToken);
        SetDueTime(punchItem, busEvent.DueDate);

        await SetLibraryItemAsync(punchItem, busEvent.PunchPriorityGuid, LibraryType.PUNCHLIST_PRIORITY, cancellationToken);
        await SetLibraryItemAsync(punchItem, busEvent.PunchListSortingGuid, LibraryType.PUNCHLIST_SORTING, cancellationToken);
        await SetLibraryItemAsync(punchItem, busEvent.PunchListTypeGuid, LibraryType.PUNCHLIST_TYPE, cancellationToken);

        SetEstimate(punchItem, busEvent.Estimate);

        await SetOriginalWorkOrderAsync(punchItem, busEvent.OriginalWoGuid, cancellationToken);
        await SetWorkOrderAsync(punchItem, busEvent.WoGuid, cancellationToken);
        await SetSWCRAsync(punchItem, busEvent.SWCRGuid, cancellationToken);
        await SetDocumentAsync(punchItem, busEvent.DocumentGuid, cancellationToken);

        SetExternalItemNo(punchItem, busEvent.ExternalItemNo);
        SetMaterialRequired(punchItem, busEvent.MaterialRequired);
        SetMaterialETAUtc(punchItem, busEvent.MaterialETA);
        SetMaterialExternalNo(punchItem, busEvent.MaterialExternalNo);

        return punchItem;
    }

    private void SetMaterialExternalNo(PunchItem punchItem, string? materialExternalNo)
    {
        if (materialExternalNo is null)
        {
            return;
        }
        punchItem.MaterialExternalNo = materialExternalNo;
    }

    private void SetMaterialETAUtc(PunchItem punchItem, DateTime? materialETAUtc )
    {
        if (materialETAUtc is null)
        {
            return;
        }
        punchItem.MaterialETAUtc = materialETAUtc;
    }

    private void SetMaterialRequired(PunchItem punchItem, bool materialRequired)
    {
        if (!materialRequired)
        {
            return;
        }
        punchItem.MaterialRequired = materialRequired;
    }

    private void SetExternalItemNo(PunchItem punchItem, string? externalItemNo)
    {
        if (externalItemNo is null)
        {
            return;
        }
        punchItem.ExternalItemNo = externalItemNo;
    }

    private async Task SetDocumentAsync(
        PunchItem punchItem,
        Guid? documentGuid,
        CancellationToken cancellationToken)
    {
        if (documentGuid is null)
        {
            return;
        }

        var doc = await _documentRepository.GetAsync(documentGuid.Value, cancellationToken);
        punchItem.SetDocument(doc);
    }

    private async Task SetSWCRAsync(
        PunchItem punchItem,
        Guid? swcrGuid,
        CancellationToken cancellationToken)
    {
        if (swcrGuid is null)
        {
            return;
        }

        var swcr = await _swcrRepository.GetAsync(swcrGuid.Value, cancellationToken);
        punchItem.SetSWCR(swcr);
    }

    private async Task SetWorkOrderAsync(
        PunchItem punchItem,
        Guid? workOrderGuid,
        CancellationToken cancellationToken)
    {
        if (workOrderGuid is null)
        {
            return;
        }

        var wo = await _woRepository.GetAsync(workOrderGuid.Value, cancellationToken);
        punchItem.SetWorkOrder(wo);
    }

    private async Task SetOriginalWorkOrderAsync(
        PunchItem punchItem,
        Guid? originalWorkOrderGuid,
        CancellationToken cancellationToken)
    {
        if (originalWorkOrderGuid is null)
        {
            return;
        }

        var wo = await _woRepository.GetAsync(originalWorkOrderGuid.Value, cancellationToken);
        punchItem.SetOriginalWorkOrder(wo);
    }

    private void SetEstimate(PunchItem punchItem, string? estimate)
    {
        if (int.TryParse(estimate, out var number))
        {
            punchItem.Estimate = number;
        }
        else
        {
            throw new Exception("Message/Estimate does not have a valid format");
        }
    }

    private void SetDueTime(PunchItem punchItem, DateTime? dueTimeUtc)
    {
        if (dueTimeUtc is null)
        {
            return;
        }
        punchItem.DueTimeUtc = dueTimeUtc;
    }

    private async Task SetLibraryItemAsync(
        PunchItem punchItem,
        Guid? libraryGuid,
        LibraryType libraryType,
        CancellationToken cancellationToken)
    {
        if (libraryGuid is null)
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


public record PunchItemEvent
(
    string EventType,
    string Plant,
    Guid ProCoSysGuid,
    string ProjectName,
    Guid ProjectGuid,
    DateTime LastUpdated,
    long PunchItemNo,
    string? Description,
    long ChecklistId,
    Guid ChecklistGuid,
    string Category,
    Guid? RaisedByOrgGuid,
    string? RaisedByOrg,
    string? ClearingByOrg,
    Guid? ClearingByOrgGuid,
    DateTime? DueDate,
    string? PunchListSorting,
    Guid? PunchListSortingGuid,
    string? PunchListType,
    Guid? PunchListTypeGuid,
    string? PunchPriority,
    Guid? PunchPriorityGuid,
    string? Estimate,
    string? OriginalWoNo,
    Guid? OriginalWoGuid,
    string? WoNo,
    Guid? WoGuid,
    string? SWCRNo,
    Guid? SWCRGuid,
    string? DocumentNo,
    Guid? DocumentGuid,
    string? ExternalItemNo,
    bool MaterialRequired,
    bool IsVoided,
    DateTime? MaterialETA,
    string? MaterialExternalNo,
    DateTime? ClearedAt,
    DateTime? RejectedAt,
    DateTime? VerifiedAt,
    DateTime CreatedAt
) : IPunchListItemEventV1;

