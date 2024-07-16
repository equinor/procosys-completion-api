using System.Security.Cryptography.X509Certificates;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.TieImport.Models;
using Equinor.ProCoSys.Completion.TieImport.Validators;
using MediatR;

namespace Equinor.ProCoSys.Completion.TieImport.Mappers;

public sealed class PunchItemImportMessageToCreatePunchItem(PlantScopedImportDataContext scopedImportDataContext)
{
    public IEnumerable<ImportResult> Map(ImportResult[] messages)
    {
        for (var i = 0; i < messages.Length; i++)
        {
            var message = messages[i];
            var (command, errors) = Map(message.Message!);
            yield return message with { Command = command, Errors = [..message.Errors, ..errors] };
        }
    }

    private (CreatePunchItemCommand? Command, IReadOnlyCollection<ImportError> Errors) Map(PunchItemImportMessage message)
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

        var (references, errors) = scopedImportDataContext.GetPunchItemImportMessageReferences(message);
        if (errors.Count != 0)
        {
            return (null, errors);
        }

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

        return (command, Array.Empty<ImportError>());
    }
}
