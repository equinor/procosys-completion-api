using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.TieImport.Models;
using Equinor.ProCoSys.Completion.TieImport.Validators;

namespace Equinor.ProCoSys.Completion.TieImport.Mappers;

public sealed class PunchItemImportMessageToCreateCommand(PlantScopedImportDataContext scopedImportDataContext)
    : ICommandMapper
{
    private (CreatePunchItemCommand? Command, IReadOnlyCollection<ImportError> Errors) Map(
        PunchItemImportMessage message)
    {
        var validator = new PunchItemImportMessageValidator(scopedImportDataContext);
        var validationResult = validator.Validate(message);
        if (!validationResult.IsValid)
        {
            return (null, validationResult
                .Errors
                .Select(x => message.ToImportError(x.ErrorMessage))
                .ToArray());
        }

        var referencesService = new CommandReferencesService(scopedImportDataContext);
        var references = referencesService.GetCreatePunchItemReferences(message);
        if (references.Errors.Length != 0)
        {
            return (null, references.Errors);
        }

        var command = new CreatePunchItemCommand(
            message.Category!.Value,
            message.Description.Value ?? string.Empty,
            references.ProjectGuid,
            references.CheckListGuid,
            references.RaisedByOrgGuid,
            references.ClearedByOrgGuid,
            null,
            message.DueDate.Value,
            null,
            null,
            references.TypeGuid,
            null,
            null,
            null,
            null,
            null,
            message.ExternalPunchItemNo,
            false, // FIXME
            message.MaterialEta.Value,
            message.MaterialNo.Value
        );

        return (command, Array.Empty<ImportError>());
    }

    public ImportResult Map(ImportResult message)
    {
        if (message.Message is null)
        {
            return message;
        }

        var (command, errors) = Map(message.Message);

        return message with { Command = command, Errors = [..message.Errors, ..errors] };
    }
}
