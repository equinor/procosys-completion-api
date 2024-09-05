using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport;

public interface IImportHandler
{
    Task<TIMessageResult> Handle(TIInterfaceMessage message);
}
