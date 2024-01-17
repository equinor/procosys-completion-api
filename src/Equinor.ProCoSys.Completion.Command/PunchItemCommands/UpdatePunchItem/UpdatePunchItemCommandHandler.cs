using System;
using System.Collections.Generic;
using System.Linq;
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
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Equinor.ProCoSys.Completion.MessageContracts.PunchItem;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;

public class UpdatePunchItemCommandHandler : IRequestHandler<UpdatePunchItemCommand, Result<string>>
{
    private readonly IPunchItemRepository _punchItemRepository;
    private readonly ILibraryItemRepository _libraryItemRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly ISWCRRepository _swcrRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ISyncToPCS4Service _syncToPCS4Service;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPunchEventPublisher _punchEventPublisher;
    private readonly IHistoryEventPublisher _historyEventPublisher;
    private readonly ILogger<UpdatePunchItemCommandHandler> _logger;

    public UpdatePunchItemCommandHandler(
        IPunchItemRepository punchItemRepository,
        ILibraryItemRepository libraryItemRepository,
        IPersonRepository personRepository,
        IWorkOrderRepository workOrderRepository,
        ISWCRRepository swcrRepository,
        IDocumentRepository documentRepository,
        ISyncToPCS4Service syncToPCS4Service,
        IUnitOfWork unitOfWork,
        IPunchEventPublisher punchEventPublisher,
        IHistoryEventPublisher historyEventPublisher,
        ILogger<UpdatePunchItemCommandHandler> logger)
    {
        _punchItemRepository = punchItemRepository;
        _libraryItemRepository = libraryItemRepository;
        _personRepository = personRepository;
        _workOrderRepository = workOrderRepository;
        _swcrRepository = swcrRepository;
        _documentRepository = documentRepository;
        _syncToPCS4Service = syncToPCS4Service;
        _unitOfWork = unitOfWork;
        _punchEventPublisher = punchEventPublisher;
        _historyEventPublisher = historyEventPublisher;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(UpdatePunchItemCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var punchItem = await _punchItemRepository.GetAsync(request.PunchItemGuid, cancellationToken);

            var changes = await PatchAsync(punchItem, request.PatchDocument, cancellationToken);

            // AuditData must be set before publishing events due to use of Created- and Modified-properties
            await _unitOfWork.SetAuditDataAsync();

            IPunchItemUpdatedV1 integrationEvent = null!;
            if (changes.Any())
            {
                integrationEvent = await _punchEventPublisher.PublishUpdatedEventAsync(punchItem, cancellationToken);
                await _historyEventPublisher.PublishUpdatedEventAsync(
                    punchItem.Plant,
                    "Punch item updated",
                    punchItem.Guid,
                    new User(punchItem.ModifiedBy!.Guid, punchItem.ModifiedBy!.GetFullName()),
                    punchItem.ModifiedAtUtc!.Value,
                    changes,
                    cancellationToken);
            }

            punchItem.SetRowVersion(request.RowVersion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (changes.Any())
            {
                await _syncToPCS4Service.SyncObjectUpdateAsync(SyncToPCS4Service.PunchItem, integrationEvent, punchItem.Plant, cancellationToken);
            }

            // todo 109356 add unit tests
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} updated", punchItem.ItemNo, punchItem.Guid);

            return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
        }
        catch (Exception)
        {
            _logger.LogError("Error occurred on update of punch item with guid {PunchItemGuid}.", request.PunchItemGuid);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<List<IProperty>> PatchAsync(
        PunchItem punchItem,
        JsonPatchDocument<PatchablePunchItem> patchDocument,
        CancellationToken cancellationToken)
    {
        var changes = new List<IProperty>();

        var propertiesToReplace = GetPropertiesToReplace(patchDocument);
        if (!propertiesToReplace.Any())
        {
            return changes;
        }

        var patchedPunchItem = new PatchablePunchItem();

        patchDocument.ApplyTo(patchedPunchItem);

        foreach (var propertyToReplace in propertiesToReplace)
        {
            switch (propertyToReplace)
            {
                case nameof(PatchablePunchItem.Description):
                    PatchDescription(punchItem, patchedPunchItem, changes);
                    break;

                case nameof(PatchablePunchItem.RaisedByOrgGuid):
                    await PatchRaisedByOrgAsync(punchItem, patchedPunchItem, changes, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.ClearingByOrgGuid):
                    await PatchClearingByOrgGuidAsync(punchItem, patchedPunchItem, changes, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.ActionByPersonOid):
                    await PatchActionByPersonAsync(punchItem, patchedPunchItem, changes, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.DueTimeUtc):
                    PatchDueTime(punchItem, patchedPunchItem, changes);
                    break;

                case nameof(PatchablePunchItem.Estimate):
                    PatchEstimate(punchItem, patchedPunchItem, changes);
                    break;

                case nameof(PatchablePunchItem.PriorityGuid):
                    await PatchPriorityAsync(punchItem, patchedPunchItem, changes, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.SortingGuid):
                    await PatchSortingAsync(punchItem, patchedPunchItem, changes, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.TypeGuid):
                    await PatchTypeAsync(punchItem, patchedPunchItem, changes, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.OriginalWorkOrderGuid):
                    await PatchOriginalWorkOrderAsync(punchItem, patchedPunchItem, changes, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.WorkOrderGuid):
                    await PatchWorkOrderAsync(punchItem, patchedPunchItem, changes, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.SWCRGuid):
                    await PatchSWCRAsync(punchItem, patchedPunchItem, changes, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.DocumentGuid):
                    await PatchDocumentAsync(punchItem, patchedPunchItem, changes, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.ExternalItemNo):
                    PatchExternalItemNo(punchItem, patchedPunchItem, changes);
                    break;

                case nameof(PatchablePunchItem.MaterialRequired):
                    PatchMaterialRequired(punchItem, patchedPunchItem, changes);
                    break;

                case nameof(PatchablePunchItem.MaterialETAUtc):
                    PatchMaterialETA(punchItem, patchedPunchItem, changes);
                    break;

                case nameof(PatchablePunchItem.MaterialExternalNo):
                    PatchMaterialExternalNo(punchItem, patchedPunchItem, changes);
                    break;

                default:
                    throw new NotImplementedException($"Patching property {propertyToReplace} not implemented");
            }
        }

        return changes;
    }

    private async Task PatchOriginalWorkOrderAsync(
        PunchItem punchItem,
        PatchablePunchItem patchedPunchItem,
        List<IProperty> changes,
        CancellationToken cancellationToken)
    {
        if (punchItem.OriginalWorkOrder?.Guid == patchedPunchItem.OriginalWorkOrderGuid)
        {
            return;
        }

        if (patchedPunchItem.OriginalWorkOrderGuid is not null)
        {
            var workOrder = await _workOrderRepository.GetAsync(patchedPunchItem.OriginalWorkOrderGuid.Value, cancellationToken);
            changes.Add(new Property<string?>(nameof(punchItem.OriginalWorkOrder),
                punchItem.OriginalWorkOrder?.No,
                workOrder.No));
            punchItem.SetOriginalWorkOrder(workOrder);
        }
        else
        {
            changes.Add(new Property<string?>(nameof(punchItem.OriginalWorkOrder),
                punchItem.OriginalWorkOrder?.No,
                null));
            punchItem.ClearOriginalWorkOrder();
        }
    }

    private async Task PatchWorkOrderAsync(
        PunchItem punchItem,
        PatchablePunchItem patchedPunchItem,
        List<IProperty> changes,
        CancellationToken cancellationToken)
    {
        if (punchItem.WorkOrder?.Guid == patchedPunchItem.WorkOrderGuid)
        {
            return;
        }

        if (patchedPunchItem.WorkOrderGuid is not null)
        {
            var workOrder = await _workOrderRepository.GetAsync(patchedPunchItem.WorkOrderGuid.Value, cancellationToken);
            changes.Add(new Property<string?>(nameof(punchItem.WorkOrder),
                punchItem.WorkOrder?.No,
                workOrder.No));
            punchItem.SetWorkOrder(workOrder);
        }
        else
        {
            changes.Add(new Property<string?>(nameof(punchItem.WorkOrder),
                punchItem.WorkOrder?.No,
                null));
            punchItem.ClearWorkOrder();
        }
    }

    private async Task PatchSWCRAsync(
        PunchItem punchItem,
        PatchablePunchItem patchedPunchItem,
        List<IProperty> changes,
        CancellationToken cancellationToken)
    {
        if (punchItem.SWCR?.Guid == patchedPunchItem.SWCRGuid)
        {
            return;
        }

        if (patchedPunchItem.SWCRGuid is not null)
        {
            var swcr = await _swcrRepository.GetAsync(patchedPunchItem.SWCRGuid.Value, cancellationToken);
            changes.Add(new Property<int?>(nameof(punchItem.SWCR),
                punchItem.SWCR?.No,
                swcr.No));
            punchItem.SetSWCR(swcr);
        }
        else
        {
            changes.Add(new Property<int?>(nameof(punchItem.SWCR),
                punchItem.SWCR?.No,
                null));
            punchItem.ClearSWCR();
        }
    }

    private async Task PatchDocumentAsync(
        PunchItem punchItem,
        PatchablePunchItem patchedPunchItem,
        List<IProperty> changes,
        CancellationToken cancellationToken)
    {
        if (punchItem.Document?.Guid == patchedPunchItem.DocumentGuid)
        {
            return;
        }

        if (patchedPunchItem.DocumentGuid is not null)
        {
            var document = await _documentRepository.GetAsync(patchedPunchItem.DocumentGuid.Value, cancellationToken);
            changes.Add(new Property<string?>(nameof(punchItem.Document),
                punchItem.Document?.No,
                document.No));
            punchItem.SetDocument(document);
        }
        else
        {
            changes.Add(new Property<string?>(nameof(punchItem.Document),
                punchItem.Document?.No,
                null));
            punchItem.ClearDocument();
        }
    }

    private async Task PatchActionByPersonAsync(
        PunchItem punchItem,
        PatchablePunchItem patchedPunchItem,
        List<IProperty> changes,
        CancellationToken cancellationToken)
    {
        if (punchItem.ActionBy?.Guid == patchedPunchItem.ActionByPersonOid)
        {
            return;
        }

        if (patchedPunchItem.ActionByPersonOid is not null)
        {
            var person = await _personRepository.GetOrCreateAsync(patchedPunchItem.ActionByPersonOid.Value, cancellationToken);
            changes.Add(new Property<User?>(nameof(punchItem.ActionBy),
                punchItem.ActionBy is null ? null : new User(punchItem.ActionBy.Guid, punchItem.ActionBy.GetFullName()),
                new User(person.Guid, person.GetFullName())));
            punchItem.SetActionBy(person);
        }
        else
        {
            changes.Add(new Property<User?>(nameof(punchItem.ActionBy),
                new User(punchItem.ActionBy!.Guid, punchItem.ActionBy!.GetFullName()),
                null));
            punchItem.ClearActionBy();
        }
    }

    private async Task PatchTypeAsync(
        PunchItem punchItem,
        PatchablePunchItem patchedPunchItem,
        List<IProperty> changes,
        CancellationToken cancellationToken)
    {
        if (punchItem.Type?.Guid == patchedPunchItem.TypeGuid)
        {
            return;
        }

        if (patchedPunchItem.TypeGuid is not null)
        {
            var libraryItem = await _libraryItemRepository.GetByGuidAndTypeAsync(
                patchedPunchItem.TypeGuid.Value,
                LibraryType.PUNCHLIST_TYPE,
                cancellationToken);
            changes.Add(new Property<string?>(nameof(punchItem.Type),
                punchItem.Type?.Code,
                libraryItem.Code));
            punchItem.SetType(libraryItem);
        }
        else
        {
            changes.Add(new Property<string?>(nameof(punchItem.Type),
                punchItem.Type!.Code,
                null));
            punchItem.ClearType();
        }
    }

    private async Task PatchSortingAsync(
        PunchItem punchItem,
        PatchablePunchItem patchedPunchItem,
        List<IProperty> changes,
        CancellationToken cancellationToken)
    {
        if (punchItem.Sorting?.Guid == patchedPunchItem.SortingGuid)
        {
            return;
        }

        if (patchedPunchItem.SortingGuid is not null)
        {
            var libraryItem = await _libraryItemRepository.GetByGuidAndTypeAsync(
                patchedPunchItem.SortingGuid.Value,
                LibraryType.PUNCHLIST_SORTING,
                cancellationToken);
            changes.Add(new Property<string?>(nameof(punchItem.Sorting),
                punchItem.Sorting?.Code,
                libraryItem.Code));
            punchItem.SetSorting(libraryItem);
        }
        else
        {
            changes.Add(new Property<string?>(nameof(punchItem.Sorting),
                punchItem.Sorting!.Code,
                null));
            punchItem.ClearSorting();
        }
    }

    private async Task PatchPriorityAsync(
        PunchItem punchItem,
        PatchablePunchItem patchedPunchItem,
        List<IProperty> changes,
        CancellationToken cancellationToken)
    {
        if (punchItem.Priority?.Guid == patchedPunchItem.PriorityGuid)
        {
            return;
        }

        if (patchedPunchItem.PriorityGuid is not null)
        {
            var libraryItem = await _libraryItemRepository.GetByGuidAndTypeAsync(
                patchedPunchItem.PriorityGuid.Value,
                LibraryType.PUNCHLIST_PRIORITY,
                cancellationToken);
            changes.Add(new Property<string?>(nameof(punchItem.Priority),
                punchItem.Priority?.Code,
                libraryItem.Code));
            punchItem.SetPriority(libraryItem);
        }
        else
        {
            changes.Add(new Property<string?>(nameof(punchItem.Priority),
                punchItem.Priority!.Code,
                null));
            punchItem.ClearPriority();
        }
    }

    private async Task PatchClearingByOrgGuidAsync(
        PunchItem punchItem,
        PatchablePunchItem patchedPunchItem,
        List<IProperty> changes,
        CancellationToken cancellationToken)
    {
        if (punchItem.ClearingByOrg.Guid == patchedPunchItem.ClearingByOrgGuid)
        {
            return;
        }

        var libraryItem = await _libraryItemRepository.GetByGuidAndTypeAsync(
            patchedPunchItem.ClearingByOrgGuid,
            LibraryType.COMPLETION_ORGANIZATION,
            cancellationToken);
        changes.Add(new Property<string>(nameof(punchItem.ClearingByOrg),
            punchItem.ClearingByOrg.Code,
            libraryItem.Code));
        punchItem.SetClearingByOrg(libraryItem);
    }

    private async Task PatchRaisedByOrgAsync(
        PunchItem punchItem,
        PatchablePunchItem patchedPunchItem,
        List<IProperty> changes,
        CancellationToken cancellationToken)
    {
        if (punchItem.RaisedByOrg.Guid == patchedPunchItem.RaisedByOrgGuid)
        {
            return;
        }

        var libraryItem = await _libraryItemRepository.GetByGuidAndTypeAsync(
            patchedPunchItem.RaisedByOrgGuid,
            LibraryType.COMPLETION_ORGANIZATION,
            cancellationToken);
        changes.Add(new Property<string>(nameof(punchItem.RaisedByOrg),
            punchItem.RaisedByOrg.Code,
            libraryItem.Code));
        punchItem.SetRaisedByOrg(libraryItem);
    }

    private static void PatchDescription(PunchItem punchItem, PatchablePunchItem patchedPunchItem, List<IProperty> changes)
    {
        if (punchItem.Description == patchedPunchItem.Description)
        {
            return;
        }

        changes.Add(new Property<string>(nameof(punchItem.Description),
            punchItem.Description,
            patchedPunchItem.Description));
        punchItem.Description = patchedPunchItem.Description;
    }

    private static void PatchDueTime(PunchItem punchItem, PatchablePunchItem patchedPunchItem, List<IProperty> changes)
    {
        if (punchItem.DueTimeUtc == patchedPunchItem.DueTimeUtc)
        {
            return;
        }

        changes.Add(new Property<DateTime?>(nameof(punchItem.DueTimeUtc),
            punchItem.DueTimeUtc,
            patchedPunchItem.DueTimeUtc));
        punchItem.DueTimeUtc = patchedPunchItem.DueTimeUtc;
    }

    private static void PatchEstimate(PunchItem punchItem, PatchablePunchItem patchedPunchItem, List<IProperty> changes)
    {
        if (punchItem.Estimate == patchedPunchItem.Estimate)
        {
            return;
        }

        changes.Add(new Property<int?>(nameof(punchItem.Estimate),
            punchItem.Estimate,
            patchedPunchItem.Estimate));
        punchItem.Estimate = patchedPunchItem.Estimate;
    }

    private static void PatchExternalItemNo(PunchItem punchItem, PatchablePunchItem patchedPunchItem, List<IProperty> changes)
    {
        if (punchItem.ExternalItemNo == patchedPunchItem.ExternalItemNo)
        {
            return;
        }

        changes.Add(new Property<string?>(nameof(punchItem.ExternalItemNo),
            punchItem.ExternalItemNo,
            patchedPunchItem.ExternalItemNo));
        punchItem.ExternalItemNo = patchedPunchItem.ExternalItemNo;
    }

    private static void PatchMaterialRequired(PunchItem punchItem, PatchablePunchItem patchedPunchItem, List<IProperty> changes)
    {
        if (punchItem.MaterialRequired == patchedPunchItem.MaterialRequired)
        {
            return;
        }

        changes.Add(new Property<bool>(nameof(punchItem.MaterialRequired),
            punchItem.MaterialRequired,
            patchedPunchItem.MaterialRequired));
        punchItem.MaterialRequired = patchedPunchItem.MaterialRequired;
    }

    private static void PatchMaterialETA(PunchItem punchItem, PatchablePunchItem patchedPunchItem, List<IProperty> changes)
    {
        if (punchItem.MaterialETAUtc == patchedPunchItem.MaterialETAUtc)
        {
            return;
        }

        changes.Add(new Property<DateTime?>(nameof(punchItem.MaterialETAUtc),
            punchItem.MaterialETAUtc,
            patchedPunchItem.MaterialETAUtc));
        punchItem.MaterialETAUtc = patchedPunchItem.MaterialETAUtc;
    }

    private static void PatchMaterialExternalNo(PunchItem punchItem, PatchablePunchItem patchedPunchItem, List<IProperty> changes)
    {
        if (punchItem.MaterialExternalNo == patchedPunchItem.MaterialExternalNo)
        {
            return;
        }

        changes.Add(new Property<string?>(nameof(punchItem.MaterialExternalNo),
            punchItem.MaterialExternalNo,
            patchedPunchItem.MaterialExternalNo));
        punchItem.MaterialExternalNo = patchedPunchItem.MaterialExternalNo;
    }

    private static List<string> GetPropertiesToReplace(JsonPatchDocument<PatchablePunchItem> patchDocument)
        => patchDocument.Operations
            .Where(op => op.OperationType == OperationType.Replace)
            .Select(op => op.path.TrimStart('/')).ToList();
}
