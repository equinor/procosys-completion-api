using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.TieImport.Models;
using Equinor.ProCoSys.Completion.TieImport.Validators;

namespace Equinor.ProCoSys.Completion.TieImport.Mappers;

public sealed class PunchItemImportMessageToDeleteCommand(PlantScopedImportDataContext scopedImportDataContext)
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

    public ImportResult Map(ImportResult message)
    {
        if (message.Message is null) return message;
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

        message = message with
        {
            Command = new DeletePunchItemCommand(references.PunchItem!.Guid,
                references.PunchItem.RowVersion.ConvertToString())
        };

        return message;
    }
}
