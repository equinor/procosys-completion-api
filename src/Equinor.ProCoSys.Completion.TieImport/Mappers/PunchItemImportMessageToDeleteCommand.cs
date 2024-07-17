using Equinor.ProCoSys.Completion.TieImport.Models;

namespace Equinor.ProCoSys.Completion.TieImport.Mappers;

public sealed class PunchItemImportMessageToDeleteCommand(PlantScopedImportDataContext scopedImportDataContext) : ICommandMapper
{


    public ImportResult Map(ImportResult message)
    {
        return message;
    }
}
