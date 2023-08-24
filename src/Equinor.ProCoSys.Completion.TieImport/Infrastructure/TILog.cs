using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Infrastructure;

public static class TILog
{
    public static TILogEntry CreateEntry(string message, string? details = null)
        => CreateTieLogEntry(LogTypes.Error, message, details);

    public static TILogEntry CreateEntry(TIObject tieObject, string message, string? details = null)
        => CreateTieLogEntry(LogTypes.Error, tieObject.GetLogFriendlyStringForTieLog() + " " + message, details);

    public static TILogEntry CreateWarningEntry(TIObject tieObject, string message, string? details = null)
        => CreateTieLogEntry(LogTypes.Warning, tieObject.GetLogFriendlyStringForTieLog() + " " + message, details);

    private static TILogEntry CreateTieLogEntry(LogTypes logType, string logEntry, string? details) => new()
    {
        Guid = Guid.NewGuid(),
        LogScopeByEnum = LogScopes.General,
        LogTypeByEnum = logType,
        TimeStamp = DateTime.Now,
        LogDescription = logEntry,
        Xml = details
    };
}
