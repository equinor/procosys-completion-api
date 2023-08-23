using Statoil.TI.InterfaceServices.Logging;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Infrastructure;

public static class ExceptionExtensions
{
    public static TIMessageResult ToMessageResult(this Exception e) => new()
    {
        Result = MessageResults.Failed,
        ErrorMessage = e.Message,
        ErrorDetails = e.StackTrace,
        Logs = { TILog.CreateEntry(e.Message, e.StackTrace) }
    };
}
