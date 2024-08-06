using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport;

public interface IImportHandler
{
    Task<TIResponseFrame> Handle(TIInterfaceMessage message);
}
