using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.TieImport.Validators;

namespace Equinor.ProCoSys.Completion.TieImport.Mappers;

public sealed class PunchItemImportMessageToCreatePunchItem(PlantScopedImportDataContext scopedImportDataContext)
{
    public (IReadOnlyCollection<CreatePunchItemCommand> Commands, IReadOnlyCollection<ImportError> Errors) Map(
        IReadOnlyCollection<PunchItemImportMessage> messages)
    {
        var results = messages
            .Select(Map)
            .ToArray();

        var commands = results
            .Where(x => x.Command is not null)
            .Select(x => x.Command!)
            .ToArray();
        
        var errors = results
            .SelectMany(x => x.Errors)
            .ToArray();

        return (commands, errors);
    }

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

        var references = scopedImportDataContext.GetPunchItemImportMessageReferences(message);

        var command = new CreatePunchItemCommand(
            message.Category!.Value,
            message.Description.Value ?? string.Empty,
            references.ProjectGuid,
            references.CheckListGuid,
            references.RaisedByOrgGuid,
            references.ClearingByOrgGuid,
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

        return (command, []);
    }
}
