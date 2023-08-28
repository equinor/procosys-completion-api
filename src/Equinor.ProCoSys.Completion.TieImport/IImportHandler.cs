using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport;

public interface IImportHandler
{
    TIResponseFrame Handle(TIInterfaceMessage message);
}
