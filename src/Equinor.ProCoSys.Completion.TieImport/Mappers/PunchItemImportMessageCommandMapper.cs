using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.TieImport.Models;

namespace Equinor.ProCoSys.Completion.TieImport.Mappers;

public sealed class PunchItemImportMessageCommandMapper(PlantScopedImportDataContext context)
{
    private readonly Dictionary<string, ICommandMapper> _commandMappersByMethod = new()
    {
        {PunchObjectAttributes.Methods.Create, new PunchItemImportMessageToCreateCommand(context)},
        {PunchObjectAttributes.Methods.Update, new PunchItemImportMessageToUpdateCommand(context)},
        {PunchObjectAttributes.Methods.Delete, new PunchItemImportMessageToDeleteCommand(context)},
    };
    public IEnumerable<ImportResult> Map(ImportResult[] importResults)
    {
        foreach (var importResult in importResults)
        {
            var commandMapper = _commandMappersByMethod[importResult.TiObject.Method];
            yield return commandMapper.SetCommandToImportResult(importResult);
        }
    }
}

public interface ICommandMapper
{
    public ImportResult SetCommandToImportResult(ImportResult importResult);
}
