using System.Linq.Expressions;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Microsoft.AspNetCore.JsonPatch;

namespace Equinor.ProCoSys.Completion.TieImport.Mappers;

public static class ImportUpdateHelper
{
    public static JsonPatchDocument<PatchablePunchItem> CreateJsonPatchDocument(PunchItemImportMessage? message,
        PunchItem punchItem,
        CommandReferences references)
    {
        if (message is null)
        {
            return new JsonPatchDocument<PatchablePunchItem>();
        }
        var jsonPatchDocument = new JsonPatchDocument<PatchablePunchItem>();

        AddPatchIfChanged(
            jsonPatchDocument,
            message.Description.HasValue && message.Description.Value != punchItem.Description,
            x => x.Description,
            message.Description.Value);

        AddPatchIfChanged(
            jsonPatchDocument,
            message.RaisedByOrganization.HasValue &&
            references.RaisedByOrgGuid != punchItem.RaisedByOrg.Guid,
            x => x.RaisedByOrgGuid,
            references.RaisedByOrgGuid);

        AddPatchIfChanged(
            jsonPatchDocument,
            message.ClearedByOrganization.HasValue &&
            references.ClearedByOrgGuid != punchItem.ClearingByOrg.Guid,
            x => x.ClearingByOrgGuid,
            references.ClearedByOrgGuid);

        AddPatchIfChanged(
            jsonPatchDocument,
            message.DueDate.HasValue && message.DueDate.Value != punchItem.DueTimeUtc,
            x => x.DueTimeUtc,
            message.DueDate.Value);

        AddPatchIfChanged(
            jsonPatchDocument,
            message.PunchListType.HasValue && references.TypeGuid != null && (punchItem.Type == null ||
                                                                              punchItem.Type.Guid !=
                                                                              references.TypeGuid),
            x => x.TypeGuid,
            references.TypeGuid);

        AddPatchIfChanged(
            jsonPatchDocument,
            message.ExternalPunchItemNo != punchItem!.ExternalItemNo,
            x => x.ExternalItemNo,
            message.ExternalPunchItemNo);

        AddPatchIfChanged(
            jsonPatchDocument,
            message.MaterialEta.HasValue && message.MaterialEta.Value != punchItem.MaterialETAUtc,
            x => x.MaterialETAUtc,
            message.MaterialEta.Value);

        AddPatchIfChanged(
            jsonPatchDocument,
            message.MaterialNo.HasValue && message.MaterialNo.Value != punchItem.MaterialExternalNo,
            x => x.MaterialExternalNo,
            message.MaterialNo.Value);

        return jsonPatchDocument;
    }

    private static void AddPatchIfChanged<T>(
        JsonPatchDocument<PatchablePunchItem> patchDocument,
        bool condition,
        Expression<Func<PatchablePunchItem, T>> path,
        T value)
    {
        if (condition)
        {
            patchDocument.Replace(path, value);
        }
    }

}
