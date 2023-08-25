using Equinor.ProCoSys.Completion.TieImport.Infrastructure.Pcs;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Infrastructure;

public interface IMessageInspector
{
    string ExtractPlant(TIInterfaceMessage message);

    void CheckForScriptInjection(TIObject tieObject);

    void UpdateImportOptions(IPcsObjectIn pcsObject, TIInterfaceMessage message);
}
