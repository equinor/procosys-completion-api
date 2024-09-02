using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.TieImport.Models;
using Equinor.ProCoSys.Completion.TieImport.Validators;

namespace Equinor.ProCoSys.Completion.TieImport.Mappers;

public sealed class PunchItemImportMessageToDeleteCommand(PlantScopedImportDataContext scopedImportDataContext)
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

    public ImportResult SetCommandToImportResult(ImportResult importResult)
    {
        if (importResult.Message is null) return importResult;
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

        importResult = importResult with
        {
            // Command = new DeletePunchItemCommand(references.PunchItem!.Guid,
            //     references.PunchItem.RowVersion.ConvertToString())
        };

        return importResult;
    }
}
