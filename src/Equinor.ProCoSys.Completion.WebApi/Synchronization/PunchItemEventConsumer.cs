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
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class PunchItemEventConsumer(
    ILogger<PunchItemEventConsumer> logger,
    IPlantSetter plantSetter,
    IPersonRepository personRepository,
    IPunchItemRepository punchItemRepository,
    IProjectRepository projectRepository,
    ILibraryItemRepository libraryItemRepository,
    IDocumentRepository documentRepository,
    ISWCRRepository swcrRepository,
    IWorkOrderRepository woRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserSetter currentUserSetter,
    IOptionsMonitor<ApplicationOptions> applicationOptions)
    : IConsumer<PunchItemEvent>
{

    public async Task Consume(ConsumeContext<PunchItemEvent> context)
    {
        var busEvent = context.Message;

        ValidateMessage(busEvent);
        plantSetter.SetPlant(busEvent.Plant);

        if (await punchItemRepository.ExistsAsync(busEvent.ProCoSysGuid, context.CancellationToken))
        {
            var punchItem = await punchItemRepository.GetAsync(busEvent.ProCoSysGuid, context.CancellationToken);
            await MapPunchItemEventToPunchItem(busEvent, punchItem, context.CancellationToken);
        }
        else
        {
            var punchItem = await CreatePunchItem(busEvent, context.CancellationToken);
            punchItemRepository.Add(punchItem);
        }
        
        currentUserSetter.SetCurrentUserOid(applicationOptions.CurrentValue.ObjectId);
        await unitOfWork.SaveChangesFromSyncAsync(context.CancellationToken);

        logger.LogInformation($"{nameof(PunchItemEvent)} Message Consumed: {{MessageId}} \n Guid {{Guid}} \n {{No}}",
            context.MessageId, busEvent.ProCoSysGuid, busEvent.PunchItemNo);
    }

    private static void ValidateMessage(PunchItemEvent busEvent)
    {
        if (busEvent.ProCoSysGuid == Guid.Empty)
        {
            throw new Exception($"{nameof(PunchItemEvent)} is missing {nameof(PunchItemEvent.ProCoSysGuid)}");
        }

        if (busEvent.CreatedByGuid == Guid.Empty)
        {
            throw new Exception($"{nameof(PunchItemEvent)} is missing {nameof(PunchItemEvent.CreatedByGuid)}");
        }

        if (string.IsNullOrEmpty(busEvent.Plant))
        {
            throw new Exception($"{nameof(PunchItemEvent)} is missing {nameof(PunchItemEvent.Plant)}");
        }

        if (string.IsNullOrEmpty(busEvent.Description))
        {
            throw new Exception($"{nameof(PunchItemEvent)} is missing {nameof(PunchItemEvent.Description)}");
        }
    }

    private async Task<PunchItem> CreatePunchItem(IPunchListItemEventV1 busEvent, CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetAsync(busEvent.ProjectGuid, cancellationToken);

        var raisedByOrg = busEvent.RaisedByOrgGuid.HasValue ? await libraryItemRepository.GetAsync(
            busEvent.RaisedByOrgGuid.Value,
            cancellationToken) : throw new Exception($"{nameof(PunchItemEvent)} is missing RaisedByOrgGuid");
        var clearingByOrg = busEvent.ClearingByOrgGuid.HasValue ? await libraryItemRepository.GetAsync(
            busEvent.ClearingByOrgGuid.Value, cancellationToken) : throw new Exception($"{nameof(PunchItemEvent)} is missing ClearingByOrgGuid");

        var punchItem = new PunchItem(
            busEvent.Plant,
            project,
            busEvent.ChecklistGuid,
            Enum.Parse<Category>(busEvent.Category),
            busEvent.Description!,
            raisedByOrg,
            clearingByOrg,
            busEvent.ProCoSysGuid);
        
        await SetSyncProperties(punchItem, busEvent, cancellationToken);
        await MapPunchItemEventToPunchItem(busEvent, punchItem, cancellationToken);

        return punchItem;
    }

    private async Task MapPunchItemEventToPunchItem(IPunchListItemEventV1 busEvent, PunchItem punchItem,
        CancellationToken cancellationToken)
    {
        punchItem.Description = busEvent.Description!;
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
    }

    private static void SetMaterialExternalNo(PunchItem punchItem, string? materialExternalNo) => punchItem.MaterialExternalNo = materialExternalNo;

    private static void SetMaterialETAUtc(PunchItem punchItem, DateTime? materialETAUtc ) => punchItem.MaterialETAUtc = materialETAUtc;

    private static void SetMaterialRequired(PunchItem punchItem, bool materialRequired) => punchItem.MaterialRequired = materialRequired;

    private static void SetExternalItemNo(PunchItem punchItem, string? externalItemNo) => punchItem.ExternalItemNo = externalItemNo;

    private async Task SetSyncProperties(PunchItem punchItem, IPunchListItemEventV1 busEvent,
        CancellationToken cancellationToken)
    {
        var createdBy = await personRepository.GetOrCreateAsync(busEvent.CreatedByGuid, cancellationToken);

        punchItem.SetSyncProperties(
            createdBy,
            busEvent.CreatedAt,
            busEvent.ModifiedByGuid is not null ? await personRepository.GetOrCreateAsync(busEvent.ModifiedByGuid.Value, cancellationToken) : null,
            busEvent.LastUpdated,
            busEvent.ClearedByGuid is not null ? await personRepository.GetOrCreateAsync(busEvent.ClearedByGuid.Value, cancellationToken) : null,
            busEvent.ClearedAt,
            busEvent.RejectedByGuid is not null ? await personRepository.GetOrCreateAsync(busEvent.RejectedByGuid.Value, cancellationToken) : null,
            busEvent.RejectedAt,
            busEvent.VerifiedByGuid is not null ? await personRepository.GetOrCreateAsync(busEvent.VerifiedByGuid.Value, cancellationToken) : null,
            busEvent.VerifiedAt,
            busEvent.ActionByGuid is not null ? await personRepository.GetOrCreateAsync(busEvent.ActionByGuid.Value, cancellationToken) : null
            );
    }

    private async Task SetDocumentAsync(
        PunchItem punchItem,
        Guid? documentGuid,
        CancellationToken cancellationToken)
    {
        if (documentGuid is null)
        {
            punchItem.ClearDocument();
            return;
        }

        var doc = await documentRepository.GetAsync(documentGuid.Value, cancellationToken);
        punchItem.SetDocument(doc);
    }

    private async Task SetSWCRAsync(
        PunchItem punchItem,
        Guid? swcrGuid,
        CancellationToken cancellationToken)
    {
        if (swcrGuid is null)
        {
            punchItem.ClearSWCR();
            return;
        }

        var swcr = await swcrRepository.GetAsync(swcrGuid.Value, cancellationToken);
        punchItem.SetSWCR(swcr);
    }

    private async Task SetWorkOrderAsync(
        PunchItem punchItem,
        Guid? workOrderGuid,
        CancellationToken cancellationToken)
    {
        if (workOrderGuid is null)
        {
            punchItem.ClearWorkOrder();
            return;
        }

        var wo = await woRepository.GetAsync(workOrderGuid.Value, cancellationToken);
        punchItem.SetWorkOrder(wo);
    }

    private async Task SetOriginalWorkOrderAsync(
        PunchItem punchItem,
        Guid? originalWorkOrderGuid,
        CancellationToken cancellationToken)
    {
        if (originalWorkOrderGuid is null)
        {
            punchItem.ClearOriginalWorkOrder();
            return;
        }

        var wo = await woRepository.GetAsync(originalWorkOrderGuid.Value, cancellationToken);
        punchItem.SetOriginalWorkOrder(wo);
    }

    private static void SetEstimate(PunchItem punchItem, string? estimate)
    {
        if (estimate is null)
        {
            punchItem.Estimate = null;
            return;
        }

        if (int.TryParse(estimate, out var number))
        {
            punchItem.Estimate = number;
        }
        else
        {
            throw new Exception($"{nameof(PunchItemEvent)}.{nameof(PunchItemEvent.Estimate)} does not have a valid format");
        }
    }

    private static void SetDueTime(PunchItem punchItem, DateTime? dueTimeUtc) => punchItem.DueTimeUtc = dueTimeUtc;

    private async Task SetLibraryItemAsync(
        PunchItem punchItem,
        Guid? libraryGuid,
        LibraryType libraryType,
        CancellationToken cancellationToken)
    {
        if (libraryGuid is null)
        {
            ProcessLibraryType(libraryType, punchItem);
        }
        else
        {
            var libraryItem = await libraryItemRepository.GetByGuidAndTypeAsync(libraryGuid.Value, libraryType, cancellationToken);
            ProcessLibraryType(libraryType, punchItem, libraryItem);
        }
    }

    private static void ProcessLibraryType(LibraryType libraryType, PunchItem punchItem, LibraryItem? libraryItem = null)
    {
        switch (libraryType)
        {
            case LibraryType.PUNCHLIST_PRIORITY:
                if (libraryItem is null)
                {
                    punchItem.ClearPriority();
                }
                else
                {
                    punchItem.SetPriority(libraryItem);
                }
                break;
            case LibraryType.PUNCHLIST_SORTING:
                if (libraryItem is null)
                {
                    punchItem.ClearSorting();
                }
                else
                {
                    punchItem.SetSorting(libraryItem);
                }
                break;
            case LibraryType.PUNCHLIST_TYPE:
                if (libraryItem is null)
                {
                    punchItem.ClearType();
                }
                else
                {
                    punchItem.SetType(libraryItem);
                }
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
    DateTime CreatedAt,
    Guid? ModifiedByGuid,
    Guid? ClearedByGuid,
    Guid? RejectedByGuid,
    Guid? VerifiedByGuid,
    Guid CreatedByGuid,
    Guid? ActionByGuid
) : IPunchListItemEventV1
{
}

