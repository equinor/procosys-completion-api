using System.Linq.Expressions;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportUpdatePunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.TieImport.Models;
using Equinor.ProCoSys.Completion.TieImport.Validators;
using Microsoft.AspNetCore.JsonPatch;

namespace Equinor.ProCoSys.Completion.TieImport.Mappers;

public sealed class PunchItemImportMessageToUpdateCommand(PlantScopedImportDataContext scopedImportDataContext)
    : ICommandMapper
{
    private ImportError[] Validate(PunchItemImportMessage message)
    {
        var validator = new PunchItemImportMessageValidator();
        var validationResult = validator.Validate(message);
        return validationResult
            .Errors
            .Select(x => message.ToImportError(x.ErrorMessage))
            .ToArray();
    }

    private static ImportUpdatePunchItemCommand? MapToCommand(ImportResult message,
        UpdatePunchReferences references)
    {
        var patchDocument = CreateJsonPatchDocument(message.Message, references);
        var clearedBy = references.ClearedBy;
        var verifiedBy = references.VerifiedBy;
        var rejectedBy = references.RejectedBy;
        var category = message.Message?.Category;

        return new ImportUpdatePunchItemCommand(
            message.TiObject.Guid,
            references.ProjectGuid,
            message.TiObject.Site,
            references.PunchItem!.Guid,
            patchDocument,
            category,
            clearedBy,
            verifiedBy,
            rejectedBy,
            references.PunchItem!.RowVersion.ConvertToString());
    }

    private static JsonPatchDocument<PatchablePunchItem> CreateJsonPatchDocument(PunchItemImportMessage? message,
        UpdatePunchReferences references)
    {
        if (message is null)
        {
            return new JsonPatchDocument<PatchablePunchItem>();
        }
        var jsonPatchDocument = new JsonPatchDocument<PatchablePunchItem>();

        AddPatchIfChanged(
            jsonPatchDocument,
            message.Description.HasValue && message.Description.Value != references.PunchItem!.Description,
            x => x.Description,
            message.Description.Value);

        AddPatchIfChanged(
            jsonPatchDocument,
            message.RaisedByOrganization.HasValue &&
            references.RaisedByOrgGuid != references.PunchItem!.RaisedByOrg.Guid,
            x => x.RaisedByOrgGuid,
            references.RaisedByOrgGuid);

        AddPatchIfChanged(
            jsonPatchDocument,
            message.ClearedByOrganization.HasValue &&
            references.ClearedByOrgGuid != references.PunchItem!.ClearingByOrg.Guid,
            x => x.ClearingByOrgGuid,
            references.ClearedByOrgGuid);

        AddPatchIfChanged(
            jsonPatchDocument,
            message.DueDate.HasValue && message.DueDate.Value != references.PunchItem!.DueTimeUtc,
            x => x.DueTimeUtc,
            message.DueDate.Value);

        AddPatchIfChanged(
            jsonPatchDocument,
            message.PunchListType.HasValue && references.TypeGuid != null && (references.PunchItem!.Type == null ||
                                                                              references.PunchItem.Type.Guid !=
                                                                              references.TypeGuid),
            x => x.TypeGuid,
            references.TypeGuid);

        AddPatchIfChanged(
            jsonPatchDocument,
            message.ExternalPunchItemNo != references.PunchItem!.ExternalItemNo,
            x => x.ExternalItemNo,
            message.ExternalPunchItemNo);

        AddPatchIfChanged(
            jsonPatchDocument,
            message.MaterialEta.HasValue && message.MaterialEta.Value != references.PunchItem!.MaterialETAUtc,
            x => x.MaterialETAUtc,
            message.MaterialEta.Value);

        AddPatchIfChanged(
            jsonPatchDocument,
            message.MaterialNo.HasValue && message.MaterialNo.Value != references.PunchItem!.MaterialExternalNo,
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

    public ImportResult Map(ImportResult message)
    {
        if (message.Message is null)
        {
            return message;
        }
        
        var errors = Validate(message.Message);
        if (errors.Length != 0)
        {
            return message with { Errors = [..message.Errors, ..errors] };
        }

        var referencesService = new CommandReferencesService(scopedImportDataContext);
        var references = referencesService.GetUpdatePunchItemReferences(message.Message);
        if (references.Errors.Length != 0)
        {
            return message with { Errors = [..message.Errors, ..references.Errors] };
        }


        var command = MapToCommand(message, references);

        return message with { Command = command };
    }
}
