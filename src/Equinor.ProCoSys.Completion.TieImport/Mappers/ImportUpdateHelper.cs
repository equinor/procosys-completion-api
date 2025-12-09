using System.Linq.Expressions;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.TieImport.Models;
using Equinor.ProCoSys.Completion.TieImport.References;
using Microsoft.AspNetCore.JsonPatch;

namespace Equinor.ProCoSys.Completion.TieImport.Mappers;

public static class ImportUpdateHelper
{
    public static JsonPatchDocument<PatchablePunchItem> CreateJsonPatchDocument(PunchItemImportMessage message,
        PunchItem punchItem,
        CommandReferences references)
    {
        var jsonPatchDocument = new JsonPatchDocument<PatchablePunchItem>();

        AddPatchIfChanged(
            jsonPatchDocument,
            message.Description.HasValue && message.Description.Value != punchItem.Description,
            x => x.Description,
            message.Description.Value);

        AddPatchIfChanged(
            jsonPatchDocument,
            message.RaisedByOrganization.HasValue &&
            punchItem.RaisedByOrg != null &&
            references.RaisedByOrgGuid != punchItem.RaisedByOrg.Guid,
            x => x.RaisedByOrgGuid,
            references.RaisedByOrgGuid);

        AddPatchIfChanged(
            jsonPatchDocument,
            message.ClearedByOrganization.HasValue &&
            punchItem.ClearingByOrg != null &&
            references.ClearedByOrgGuid != punchItem.ClearingByOrg.Guid,
            x => x.ClearingByOrgGuid,
            references.ClearedByOrgGuid);

        // Handle DueDate - either set to new value or clear if marked as {NULL}
        AddPatchIfChanged(
            jsonPatchDocument,
            message.DueDate.ShouldBeNull
                ? punchItem.DueTimeUtc != null
                : message.DueDate.HasValue &&
                  message.DueDate.Value != punchItem.DueTimeUtc,
            x => x.DueTimeUtc,
            message.DueDate.ShouldBeNull ? null : message.DueDate.Value);

        // Handle PunchListType - either set to new value or clear if marked as {NULL}
        AddPatchIfChanged(
            jsonPatchDocument,
            message.PunchListType.ShouldBeNull
                ? punchItem.Type != null
                : message.PunchListType.HasValue &&
                  references.TypeGuid.HasValue &&
                  (punchItem.Type == null || punchItem.Type.Guid != references.TypeGuid.Value),
            x => x.TypeGuid,
            message.PunchListType.ShouldBeNull ? null : references.TypeGuid);

        // Handle Sorting - either set to new value or clear if marked as {NULL}
        AddPatchIfChanged(
            jsonPatchDocument,
            message.Sorting.ShouldBeNull
                ? punchItem.Sorting != null
                : message.Sorting.HasValue &&
                  references.SortingGuid.HasValue &&
                  (punchItem.Sorting == null || punchItem.Sorting.Guid != references.SortingGuid.Value),
            x => x.SortingGuid,
            message.Sorting.ShouldBeNull ? null : references.SortingGuid);

        // Handle Priority - either set to new value or clear if marked as {NULL}
        AddPatchIfChanged(
            jsonPatchDocument,
            message.Priority.ShouldBeNull
                ? punchItem.Priority != null
                : message.Priority.HasValue &&
                  references.PriorityGuid.HasValue &&
                  (punchItem.Priority == null || punchItem.Priority.Guid != references.PriorityGuid.Value),
            x => x.PriorityGuid,
            message.Priority.ShouldBeNull ? null : references.PriorityGuid);

        // Handle ActionBy - either set to new value or clear if marked as {NULL}
        AddPatchIfChanged(
            jsonPatchDocument,
            message.ActionBy.ShouldBeNull
                ? punchItem.ActionBy != null
                : message.ActionBy.HasValue &&
                  references.ActionByPersonOid.HasValue &&
                  (punchItem.ActionBy == null || punchItem.ActionBy.Guid != references.ActionByPersonOid.Value),
            x => x.ActionByPersonOid,
            message.ActionBy.ShouldBeNull ? null : references.ActionByPersonOid);

//        AddPatchIfChanged(
//            jsonPatchDocument,
//            message.ExternalPunchItemNo.HasValue && message.ExternalPunchItemNo.Value != punchItem.ExternalItemNo,
//            x => x.ExternalItemNo,
//            message.ExternalPunchItemNo.Value);

        // Handle Estimate - either set to new value or clear if marked as {NULL}
        AddPatchIfChanged(
            jsonPatchDocument,
            message.Estimate.ShouldBeNull
                ? punchItem.Estimate != null
                : message.Estimate.HasValue &&
                  message.Estimate.Value != punchItem.Estimate,
            x => x.Estimate,
            message.Estimate.ShouldBeNull ? null : message.Estimate.Value);

        // Handle MaterialEta - either set to new value or clear if marked as {NULL}
        AddPatchIfChanged(
            jsonPatchDocument,
            message.MaterialEta.ShouldBeNull
                ? punchItem.MaterialETAUtc != null
                : message.MaterialEta.HasValue &&
                  message.MaterialEta.Value != punchItem.MaterialETAUtc,
            x => x.MaterialETAUtc,
            message.MaterialEta.ShouldBeNull ? null : message.MaterialEta.Value);

        // Handle MaterialNo - either set to new value or clear if marked as {NULL}
        AddPatchIfChanged(
            jsonPatchDocument,
            message.MaterialNo.ShouldBeNull
                ? punchItem.MaterialExternalNo != null
                : message.MaterialNo.HasValue &&
                  message.MaterialNo.Value != punchItem.MaterialExternalNo,
            x => x.MaterialExternalNo,
            message.MaterialNo.ShouldBeNull ? null : message.MaterialNo.Value);

        // Handle WorkOrder - either set to new value or clear if marked as {NULL}
        AddPatchIfChanged(
            jsonPatchDocument,
            message.WorkOrderNo.ShouldBeNull
                ? punchItem.WorkOrder != null
                : message.WorkOrderNo.HasValue &&
                  references.WorkOrderGuid.HasValue &&
                  (punchItem.WorkOrder == null || punchItem.WorkOrder.Guid != references.WorkOrderGuid.Value),
            x => x.WorkOrderGuid,
            message.WorkOrderNo.ShouldBeNull ? null : references.WorkOrderGuid);

        // Handle OriginalWorkOrder - either set to new value or clear if marked as {NULL}
        AddPatchIfChanged(
            jsonPatchDocument,
            message.OriginalWorkOrderNo.ShouldBeNull
                ? punchItem.OriginalWorkOrder != null
                : message.OriginalWorkOrderNo.HasValue &&
                  references.OriginalWorkOrderGuid.HasValue &&
                  (punchItem.OriginalWorkOrder == null || punchItem.OriginalWorkOrder.Guid != references.OriginalWorkOrderGuid.Value),
            x => x.OriginalWorkOrderGuid,
            message.OriginalWorkOrderNo.ShouldBeNull ? null : references.OriginalWorkOrderGuid);

        // Handle Document - either set to new value or clear if marked as {NULL}
        AddPatchIfChanged(
            jsonPatchDocument,
            message.DocumentNo.ShouldBeNull
                ? punchItem.Document != null
                : message.DocumentNo.HasValue &&
                  references.DocumentGuid.HasValue &&
                  (punchItem.Document == null || punchItem.Document.Guid != references.DocumentGuid.Value),
            x => x.DocumentGuid,
            message.DocumentNo.ShouldBeNull ? null : references.DocumentGuid);

        // Handle SWCR - either set to new value or clear if marked as {NULL}
        AddPatchIfChanged(
            jsonPatchDocument,
            message.SwcrNo.ShouldBeNull
                ? punchItem.SWCR != null
                : message.SwcrNo.HasValue &&
                  references.SWCRGuid.HasValue &&
                  (punchItem.SWCR == null || punchItem.SWCR.Guid != references.SWCRGuid.Value),
            x => x.SWCRGuid,
            message.SwcrNo.ShouldBeNull ? null : references.SWCRGuid);

        // Handle MaterialRequired
        AddPatchIfChanged(
            jsonPatchDocument,
            message.MaterialRequired.HasValue &&
            message.MaterialRequired.Value.HasValue &&
            message.MaterialRequired.Value.Value != punchItem.MaterialRequired,
            x => x.MaterialRequired,
            message.MaterialRequired.Value ?? false);

        return jsonPatchDocument;
    }

    private static void AddPatchIfChanged<T>(
        JsonPatchDocument<PatchablePunchItem> patchDocument,
        bool condition,
        Expression<Func<PatchablePunchItem, T>> path,
        T? value)
    {
        if (condition)
        {
            patchDocument.Replace(path, value);
        }
    }
}
