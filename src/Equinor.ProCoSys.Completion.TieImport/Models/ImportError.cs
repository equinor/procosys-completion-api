namespace Equinor.ProCoSys.Completion.TieImport.Models;

public sealed record ImportError(
    Guid? MessageGuid,
    string? Method,
    string? Project,
    string? Site,
    string Message
)
{
    public override string ToString() => $"Guid {MessageGuid?.ToString() ?? "UnknownGuid"} - Method {Method ?? "UnknownMethod"} - Project {Project ?? "UnknownProject"} - Site {Site ?? "UnknownSite"} -> {Message}";
}
