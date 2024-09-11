using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Infrastructure;

public static class TILog
{
    public static TILogEntry CreateEntry(string message, string? details = null)
        => CreateTieLogEntry(LogTypes.Error, message, details);

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
