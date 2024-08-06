using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands;

public static class PunchItemPatcher
{
    public static void PatchDescription(PunchItem punchItem, PatchablePunchItem patchedPunchItem,
        List<IChangedProperty> changes)
    {
        if (punchItem.Description == patchedPunchItem.Description)
        {
            return;
        }

        changes.Add(new ChangedProperty<string>(nameof(punchItem.Description),
            punchItem.Description,
            patchedPunchItem.Description));
        punchItem.Description = patchedPunchItem.Description;
    }

    public static async Task PatchRaisedByOrgAsync(
        PunchItem punchItem,
        PatchablePunchItem patchedPunchItem,
        List<IChangedProperty> changes,
        ILibraryItemRepository libraryItemRepository,
        CancellationToken cancellationToken)
    {
        if (punchItem.RaisedByOrg.Guid == patchedPunchItem.RaisedByOrgGuid)
        {
            return;
        }

        var libraryItem = await libraryItemRepository.GetByGuidAndTypeAsync(
            patchedPunchItem.RaisedByOrgGuid,
            LibraryType.COMPLETION_ORGANIZATION,
            cancellationToken);
        changes.Add(new ChangedProperty<string>(nameof(punchItem.RaisedByOrg),
            punchItem.RaisedByOrg.ToString(),
            libraryItem.ToString()));
        punchItem.SetRaisedByOrg(libraryItem);
    }

    public static async Task PatchClearingByOrgGuidAsync(
        PunchItem punchItem,
        PatchablePunchItem patchedPunchItem,
        List<IChangedProperty> changes,
        ILibraryItemRepository libraryItemRepository,
        CancellationToken cancellationToken)
    {
        if (punchItem.ClearingByOrg.Guid == patchedPunchItem.ClearingByOrgGuid)
        {
            return;
        }

        var libraryItem = await libraryItemRepository.GetByGuidAndTypeAsync(
            patchedPunchItem.ClearingByOrgGuid,
            LibraryType.COMPLETION_ORGANIZATION,
            cancellationToken);
        changes.Add(new ChangedProperty<string>(nameof(punchItem.ClearingByOrg),
            punchItem.ClearingByOrg.ToString(),
            libraryItem.ToString()));
        punchItem.SetClearingByOrg(libraryItem);
    }

    public static void PatchDueTime(PunchItem punchItem, PatchablePunchItem patchedPunchItem,
        List<IChangedProperty> changes)
    {
        if (punchItem.DueTimeUtc == patchedPunchItem.DueTimeUtc)
        {
            return;
        }

        changes.Add(new ChangedProperty<DateTime?>(nameof(punchItem.DueTimeUtc),
            punchItem.DueTimeUtc,
            patchedPunchItem.DueTimeUtc,
            ValueDisplayType.DateTimeAsDateOnly));
        punchItem.DueTimeUtc = patchedPunchItem.DueTimeUtc;
    }

    public static void PatchEstimate(PunchItem punchItem, PatchablePunchItem patchedPunchItem,
        List<IChangedProperty> changes)
    {
        if (punchItem.Estimate == patchedPunchItem.Estimate)
        {
            return;
        }

        changes.Add(new ChangedProperty<int?>(nameof(punchItem.Estimate),
            punchItem.Estimate,
            patchedPunchItem.Estimate,
            ValueDisplayType.IntAsText));
        punchItem.Estimate = patchedPunchItem.Estimate;
    }

    public static void PatchExternalItemNo(PunchItem punchItem, PatchablePunchItem patchedPunchItem,
        List<IChangedProperty> changes)
    {
        if (punchItem.ExternalItemNo == patchedPunchItem.ExternalItemNo)
        {
            return;
        }

        changes.Add(new ChangedProperty<string?>(nameof(punchItem.ExternalItemNo),
            punchItem.ExternalItemNo,
            patchedPunchItem.ExternalItemNo));
        punchItem.ExternalItemNo = patchedPunchItem.ExternalItemNo;
    }

    public static void PatchMaterialRequired(PunchItem punchItem, PatchablePunchItem patchedPunchItem,
        List<IChangedProperty> changes)
    {
        if (punchItem.MaterialRequired == patchedPunchItem.MaterialRequired)
        {
            return;
        }

        changes.Add(new ChangedProperty<bool>(nameof(punchItem.MaterialRequired),
            punchItem.MaterialRequired,
            patchedPunchItem.MaterialRequired,
            ValueDisplayType.BoolAsYesNo));
        punchItem.MaterialRequired = patchedPunchItem.MaterialRequired;
    }

    public static void PatchMaterialETA(PunchItem punchItem, PatchablePunchItem patchedPunchItem,
        List<IChangedProperty> changes)
    {
        if (punchItem.MaterialETAUtc == patchedPunchItem.MaterialETAUtc)
        {
            return;
        }

        changes.Add(new ChangedProperty<DateTime?>(nameof(punchItem.MaterialETAUtc),
            punchItem.MaterialETAUtc,
            patchedPunchItem.MaterialETAUtc,
            ValueDisplayType.DateTimeAsDateOnly));
        punchItem.MaterialETAUtc = patchedPunchItem.MaterialETAUtc;
    }

    public static void PatchMaterialExternalNo(PunchItem punchItem, PatchablePunchItem patchedPunchItem,
        List<IChangedProperty> changes)
    {
        if (punchItem.MaterialExternalNo == patchedPunchItem.MaterialExternalNo)
        {
            return;
        }

        changes.Add(new ChangedProperty<string?>(nameof(punchItem.MaterialExternalNo),
            punchItem.MaterialExternalNo,
            patchedPunchItem.MaterialExternalNo));
        punchItem.MaterialExternalNo = patchedPunchItem.MaterialExternalNo;
    }

    public static async Task PatchActionByPersonAsync(
        PunchItem punchItem,
        PatchablePunchItem patchedPunchItem,
        List<IChangedProperty> changes,
        IPersonRepository personRepository,
        CancellationToken cancellationToken)
    {
        if (punchItem.ActionBy?.Guid == patchedPunchItem.ActionByPersonOid)
        {
            return;
        }

        if (patchedPunchItem.ActionByPersonOid is not null)
        {
            var person =
                await personRepository.GetOrCreateAsync(patchedPunchItem.ActionByPersonOid.Value, cancellationToken);
            changes.Add(new ChangedProperty<User?>(nameof(punchItem.ActionBy),
                punchItem.ActionBy is null ? null : new User(punchItem.ActionBy.Guid, punchItem.ActionBy.GetFullName()),
                new User(person.Guid, person.GetFullName()),
                ValueDisplayType.UserAsNameOnly));
            punchItem.SetActionBy(person);
        }
        else
        {
            changes.Add(new ChangedProperty<User?>(nameof(punchItem.ActionBy),
                new User(punchItem.ActionBy!.Guid, punchItem.ActionBy!.GetFullName()),
                null,
                ValueDisplayType.UserAsNameOnly));
            punchItem.ClearActionBy();
        }
    }

    public static async Task PatchTypeAsync(
        PunchItem punchItem,
        PatchablePunchItem patchedPunchItem,
        List<IChangedProperty> changes,
        ILibraryItemRepository libraryItemRepository,
        CancellationToken cancellationToken)
    {
        if (punchItem.Type?.Guid == patchedPunchItem.TypeGuid)
        {
            return;
        }

        if (patchedPunchItem.TypeGuid is not null)
        {
            var libraryItem = await libraryItemRepository.GetByGuidAndTypeAsync(
                patchedPunchItem.TypeGuid.Value,
                LibraryType.PUNCHLIST_TYPE,
                cancellationToken);
            changes.Add(new ChangedProperty<string?>(nameof(punchItem.Type),
                punchItem.Type?.ToString(),
                libraryItem.ToString()));
            punchItem.SetType(libraryItem);
        }
        else
        {
            changes.Add(new ChangedProperty<string?>(nameof(punchItem.Type),
                punchItem.Type!.ToString(),
                null));
            punchItem.ClearType();
        }
    }

    public static async Task PatchSortingAsync(
        PunchItem punchItem,
        PatchablePunchItem patchedPunchItem,
        List<IChangedProperty> changes,
        ILibraryItemRepository libraryItemRepository,
        CancellationToken cancellationToken)
    {
        if (punchItem.Sorting?.Guid == patchedPunchItem.SortingGuid)
        {
            return;
        }

        if (patchedPunchItem.SortingGuid is not null)
        {
            var libraryItem = await libraryItemRepository.GetByGuidAndTypeAsync(
                patchedPunchItem.SortingGuid.Value,
                LibraryType.PUNCHLIST_SORTING,
                cancellationToken);
            changes.Add(new ChangedProperty<string?>(nameof(punchItem.Sorting),
                punchItem.Sorting?.ToString(),
                libraryItem.ToString()));
            punchItem.SetSorting(libraryItem);
        }
        else
        {
            changes.Add(new ChangedProperty<string?>(nameof(punchItem.Sorting),
                punchItem.Sorting!.ToString(),
                null));
            punchItem.ClearSorting();
        }
    }

    public static async Task PatchPriorityAsync(
        PunchItem punchItem,
        PatchablePunchItem patchedPunchItem,
        List<IChangedProperty> changes,
        ILibraryItemRepository libraryItemRepository,
        CancellationToken cancellationToken)
    {
        if (punchItem.Priority?.Guid == patchedPunchItem.PriorityGuid)
        {
            return;
        }

        if (patchedPunchItem.PriorityGuid is not null)
        {
            var libraryItem = await libraryItemRepository.GetByGuidAndTypeAsync(
                patchedPunchItem.PriorityGuid.Value,
                LibraryType.COMM_PRIORITY,
                cancellationToken);
            changes.Add(new ChangedProperty<string?>(nameof(punchItem.Priority),
                punchItem.Priority?.ToString(),
                libraryItem.ToString()));
            punchItem.SetPriority(libraryItem);
        }
        else
        {
            changes.Add(new ChangedProperty<string?>(nameof(punchItem.Priority),
                punchItem.Priority!.ToString(),
                null));
            punchItem.ClearPriority();
        }
    }

    public static async Task PatchDocumentAsync(
        PunchItem punchItem,
        PatchablePunchItem patchedPunchItem,
        List<IChangedProperty> changes,
        IDocumentRepository documentRepository,
        CancellationToken cancellationToken)
    {
        if (punchItem.Document?.Guid == patchedPunchItem.DocumentGuid)
        {
            return;
        }

        if (patchedPunchItem.DocumentGuid is not null)
        {
            var document = await documentRepository.GetAsync(patchedPunchItem.DocumentGuid.Value, cancellationToken);
            changes.Add(new ChangedProperty<string?>(nameof(punchItem.Document),
                punchItem.Document?.No,
                document.No));
            punchItem.SetDocument(document);
        }
        else
        {
            changes.Add(new ChangedProperty<string?>(nameof(punchItem.Document),
                punchItem.Document?.No,
                null));
            punchItem.ClearDocument();
        }
    }

    public static async Task PatchSWCRAsync(
        PunchItem punchItem,
        PatchablePunchItem patchedPunchItem,
        List<IChangedProperty> changes,
        ISWCRRepository swcrRepository,
        CancellationToken cancellationToken)
    {
        if (punchItem.SWCR?.Guid == patchedPunchItem.SWCRGuid)
        {
            return;
        }

        if (patchedPunchItem.SWCRGuid is not null)
        {
            var swcr = await swcrRepository.GetAsync(patchedPunchItem.SWCRGuid.Value, cancellationToken);
            changes.Add(new ChangedProperty<int?>(nameof(punchItem.SWCR),
                punchItem.SWCR?.No,
                swcr.No,
                ValueDisplayType.IntAsText));
            punchItem.SetSWCR(swcr);
        }
        else
        {
            changes.Add(new ChangedProperty<int?>(nameof(punchItem.SWCR),
                punchItem.SWCR?.No,
                null,
                ValueDisplayType.IntAsText));
            punchItem.ClearSWCR();
        }
    }

    public static async Task PatchWorkOrderAsync(
        PunchItem punchItem,
        PatchablePunchItem patchedPunchItem,
        List<IChangedProperty> changes,
        IWorkOrderRepository workOrderRepository,
        CancellationToken cancellationToken)
    {
        if (punchItem.WorkOrder?.Guid == patchedPunchItem.WorkOrderGuid)
        {
            return;
        }

        if (patchedPunchItem.WorkOrderGuid is not null)
        {
            var workOrder =
                await workOrderRepository.GetAsync(patchedPunchItem.WorkOrderGuid.Value, cancellationToken);
            changes.Add(new ChangedProperty<string?>(nameof(punchItem.WorkOrder),
                punchItem.WorkOrder?.No,
                workOrder.No));
            punchItem.SetWorkOrder(workOrder);
        }
        else
        {
            changes.Add(new ChangedProperty<string?>(nameof(punchItem.WorkOrder),
                punchItem.WorkOrder?.No,
                null));
            punchItem.ClearWorkOrder();
        }
    }

    public static async Task PatchOriginalWorkOrderAsync(
        PunchItem punchItem,
        PatchablePunchItem patchedPunchItem,
        List<IChangedProperty> changes,
        IWorkOrderRepository workOrderRepository,
        CancellationToken cancellationToken)
    {
        if (punchItem.OriginalWorkOrder?.Guid == patchedPunchItem.OriginalWorkOrderGuid)
        {
            return;
        }

        if (patchedPunchItem.OriginalWorkOrderGuid is not null)
        {
            var workOrder =
                await workOrderRepository.GetAsync(patchedPunchItem.OriginalWorkOrderGuid.Value, cancellationToken);
            changes.Add(new ChangedProperty<string?>(nameof(punchItem.OriginalWorkOrder),
                punchItem.OriginalWorkOrder?.No,
                workOrder.No));
            punchItem.SetOriginalWorkOrder(workOrder);
        }
        else
        {
            changes.Add(new ChangedProperty<string?>(nameof(punchItem.OriginalWorkOrder),
                punchItem.OriginalWorkOrder?.No,
                null));
            punchItem.ClearOriginalWorkOrder();
        }
    }
    
    public static List<string> GetPropertiesToReplace(JsonPatchDocument<PatchablePunchItem> patchDocument)
        => patchDocument.Operations
            .Where(op => op.OperationType == OperationType.Replace)
            .Select(op => op.path.TrimStart('/')).ToList();
}
