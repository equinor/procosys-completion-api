using System.Linq.Expressions;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ClearPunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.RejectPunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnclearPunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnverifyPunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemCategory;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.VerifyPunchItem;
using Equinor.ProCoSys.Completion.Domain;
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
        var validator = new PunchItemImportMessageValidator(scopedImportDataContext);
        var validationResult = validator.Validate(message);
        return validationResult
            .Errors
            .Select(x => message.ToImportError(x.ErrorMessage))
            .ToArray();
    }

    private (object[] Command, IReadOnlyCollection<ImportError> Errors) Map(PunchItemImportMessage message)
    {
        var errors = Validate(message);
        if (errors.Length != 0)
        {
            return ([], errors);
        }

        var referencesService = new CommandReferencesService(scopedImportDataContext);
        var references = referencesService.GetUpdatePunchItemReferences(message);
        if (references.Errors.Length != 0)
        {
            return ([], references.Errors);
        }

        var actions = GetUpdateActions(message, references)
            .Distinct()
            .ToArray();
        var commands = GetCommandsForActions(actions, message, references).ToArray();

        return (commands, Array.Empty<ImportError>());
    }

    private static IEnumerable<IIsPunchItemCommand> GetCommandsForActions(IReadOnlyCollection<UpdateActions> actions,
        PunchItemImportMessage message,
        UpdatePunchReferences references)
    {
        Dictionary<UpdateActions, Func<PunchItemImportMessage, UpdatePunchReferences, IIsPunchItemCommand>>
            mapperFunctionsByActions = new()
            {
                { UpdateActions.Reject, MapToRejectPunchItemCommand },
                { UpdateActions.Unclear, MapToUnclearPunchItemCommand },
                { UpdateActions.Unverify, MapToUnverifyPunchItemCommand },
                { UpdateActions.Clear, MapToClearPunchItemCommand },
                { UpdateActions.Verify, MapToVerifyPunchItemCommand },
                { UpdateActions.Category, MapToUpdateCategoryPunchItemCommand },
                { UpdateActions.Update, MapToUpdatePunchItemCommand },
            };

        foreach (var action in actions)
        {
            yield return mapperFunctionsByActions[action](message, references);
        }
    }


    private static UpdatePunchItemCommand MapToUpdatePunchItemCommand(PunchItemImportMessage message,
        UpdatePunchReferences references)
    {
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

        return new UpdatePunchItemCommand(
            references.PunchItem.Guid,
            jsonPatchDocument,
            references.PunchItem.RowVersion.ConvertToString()
        );
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

    private static void AddPatchIfOptionalHasValue<TModel>(
        Guid value,
        Expression<Func<TModel, Guid>> path,
        JsonPatchDocument<TModel> model,
        UpdatePunchReferences references,
        Expression<Func<UpdatePunchReferences, bool>> hasChanged) where TModel : class
    {
    }

    private static void AddPatchIfOptionalHasValue<TModel, TProp>(Optional<TProp> value,
        Expression<Func<TModel, TProp>> path, JsonPatchDocument<TModel> model) where TModel : class
    {
        if (!value.HasValue) return;
        model.Replace(path, value.Value);
    }

    private static UpdatePunchItemCategoryCommand MapToUpdateCategoryPunchItemCommand(PunchItemImportMessage message,
        UpdatePunchReferences references)
        => new(references.PunchItem!.Guid, message.Category!.Value, references.PunchItem!.RowVersion.ConvertToString());

    private static VerifyPunchItemCommand MapToVerifyPunchItemCommand(PunchItemImportMessage message,
        UpdatePunchReferences references)
        => new(references.PunchItem!.Guid, references.PunchItem!.RowVersion.ConvertToString());

    private static ClearPunchItemCommand MapToClearPunchItemCommand(PunchItemImportMessage message,
        UpdatePunchReferences references)
        => new(references.PunchItem!.Guid, references.PunchItem!.RowVersion.ConvertToString());

    private static UnverifyPunchItemCommand MapToUnverifyPunchItemCommand(PunchItemImportMessage message,
        UpdatePunchReferences references)
        => new(references.PunchItem!.Guid, references.PunchItem!.RowVersion.ConvertToString());

    private static UnclearPunchItemCommand MapToUnclearPunchItemCommand(PunchItemImportMessage message,
        UpdatePunchReferences references)
        => new(references.PunchItem!.Guid, references.PunchItem!.RowVersion.ConvertToString());

    private static RejectPunchItemCommand MapToRejectPunchItemCommand(PunchItemImportMessage message,
        UpdatePunchReferences references) =>
        new(references.PunchItem!.Guid, string.Empty, [], references.PunchItem!.RowVersion.ConvertToString());

    private static IEnumerable<UpdateActions> GetUpdateActions(PunchItemImportMessage message,
        UpdatePunchReferences references)
    {
        var punch = references.PunchItem;

        if (message.RejectedBy.HasValue)
        {
            yield return HasBinaryPropertyBeenToggled(message.RejectedBy, punch?.RejectedBy?.Email,
                UpdateActions.Reject, UpdateActions.Update);
        }

        if (message.VerifiedBy.HasValue)
        {
            yield return HasBinaryPropertyBeenToggled(message.VerifiedBy, punch?.VerifiedBy?.Email,
                UpdateActions.Verify, UpdateActions.Unverify);
        }

        if (message.ClearedBy.HasValue)
        {
            yield return HasBinaryPropertyBeenToggled(message.ClearedBy, punch?.ClearedBy?.Email,
                UpdateActions.Clear, UpdateActions.Unclear);
        }

        if (message.Category.HasValue && message.Category.Value != punch?.Category)
        {
            yield return UpdateActions.Category;
        }

        yield return UpdateActions.Update;
    }

    private static UpdateActions HasBinaryPropertyBeenToggled(Optional<string?> newValue, string? oldValue,
        UpdateActions toggled, UpdateActions unToggled)
    {
        if (newValue.Value != oldValue && newValue.Value is not null)
        {
            return toggled;
        }

        if (newValue.Value != oldValue && newValue.Value is null)
        {
            return unToggled;
        }

        return UpdateActions.Update;
    }

    public ImportResult Map(ImportResult message)
    {
        if (message.Message is null)
        {
            return message;
        }

        var (commands, errors) = Map(message.Message);

        return message with { Commands = commands, Errors = [..message.Errors, ..errors] };
    }

    private enum UpdateActions
    {
        Reject,
        Unclear,
        Clear,
        Verify,
        Unverify,
        Update,
        Category
    }
}
