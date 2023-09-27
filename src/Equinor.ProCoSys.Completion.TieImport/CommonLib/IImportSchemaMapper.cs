using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.CommonLib;

public interface IImportSchemaMapper
{
    MappingResult Map(TIInterfaceMessage message);
}
