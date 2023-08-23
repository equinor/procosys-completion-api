using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.CommonLib;

public class MappingResult
{
    public TIInterfaceMessage? Message { get; }

    public TIMessageResult ErrorResult { get; }

    public bool Success => Message != null;

    public MappingResult(TIInterfaceMessage message) => Message = message;

    public MappingResult(TIMessageResult errorResult) => ErrorResult = errorResult;
}
