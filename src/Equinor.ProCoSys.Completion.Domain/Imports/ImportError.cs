using System;

namespace Equinor.ProCoSys.Completion.Domain.Imports;

public sealed record ImportError(
    Guid? Guid,
    string? Method,
    string? Project,
    string? Site,
    string Message
)
{
    public override string ToString() => $"Guid {Guid?.ToString() ?? "UnknownGuid"} - Method {Method ?? "UnknownMethod"} - Project {Project ?? "UnknownProject"} - Site {Site ?? "UnknownSite"} -> {Message}";
};
