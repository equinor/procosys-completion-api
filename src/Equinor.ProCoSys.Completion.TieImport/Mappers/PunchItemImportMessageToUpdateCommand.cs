using System.Linq.Expressions;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportUpdatePunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.TieImport.Models;
using Equinor.ProCoSys.Completion.TieImport.Validators;
using Microsoft.AspNetCore.JsonPatch;

namespace Equinor.ProCoSys.Completion.TieImport.Mappers;

public sealed class PunchItemImportMessageToUpdateCommand(PlantScopedImportDataContext scopedImportDataContext)
    : ICommandMapper, IPunchItemImportCommand
{
    private ImportError[] Validate(PunchItemImportMessage message)
    {
        var validator = new PunchItemImportMessageValidator(scopedImportDataContext);
        var validationResult = validator.Validate(message);
        return validationResult
            .Errors
            .Select(x => message.ToImportError(x.ErrorMessage))
            .ToArray();
    }

    private static ImportUpdatePunchItemCommand? MapToCommand(ImportResult message,
        UpdatePunchReferences references)
    {
        var patchDocument = CreateJsonPatchDocument(message.Message,references.PunchItem!, references);
        var clearedBy = references.ClearedBy;
        var verifiedBy = references.VerifiedBy;
        var rejectedBy = references.RejectedBy;
        var category = message.Message?.Category;

        return new ImportUpdatePunchItemCommand(
            message.Message?.TiObject.Guid ?? message.TiObject.Guid,
            references.ProjectGuid,
            message.Message?.TiObject.Site ?? message.TiObject.Site,
            references.PunchItem!.Guid,
            patchDocument,
            category,
            clearedBy,
            verifiedBy,
            rejectedBy,
            references.PunchItem!.RowVersion.ConvertToString());
    }

    public static JsonPatchDocument<PatchablePunchItem> CreateJsonPatchDocument(PunchItemImportMessage? message,
        PunchItem punchItem,
        ICommandReferences references)
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

    public ImportResult SetCommandToImportResult(ImportResult importResult)
    {
        if (importResult.Message is null)
        {
            return importResult;
        }
        
        var errors = Validate(importResult.Message);
        if (errors.Length != 0)
        {
            return importResult with { Errors = [..importResult.Errors, ..errors] };
        }

        var referencesService = new CommandReferencesService(scopedImportDataContext);
        var references = referencesService.GetUpdatePunchItemReferences(importResult.Message);
        if (references.Errors.Length != 0)
        {
            return importResult with { Errors = [..importResult.Errors, ..references.Errors] };
        }


        var command = MapToCommand(importResult, references);

        return importResult;// with { Command = command };
    }
}
