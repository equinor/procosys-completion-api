using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.TieImport.Models;

namespace Equinor.ProCoSys.Completion.TieImport.Mappers;

public sealed class PunchItemImportMessageCommandMapper(PlantScopedImportDataContext context)
{
    public IEnumerable<ImportResult> Map(ImportResult[] messages)
    {
        var mappersByMethod = new Dictionary<string, ICommandMapper>
        {
            {PunchObjectAttributes.Methods.Create, new PunchItemImportMessageToCreateCommand(context)},
            {PunchObjectAttributes.Methods.Update, new PunchItemImportMessageToUpdateCommand(context)},
            {PunchObjectAttributes.Methods.Delete, new PunchItemImportMessageToDeleteCommand(context)},
        };
        foreach (var message in messages)
        {
            yield return mappersByMethod[message.TiObject.Method].Map(message);
        }
    }
}

public interface ICommandMapper
{
    public ImportResult Map(ImportResult message);
}
