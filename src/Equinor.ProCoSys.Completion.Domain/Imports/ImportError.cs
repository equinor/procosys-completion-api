using System;

namespace Equinor.ProCoSys.Completion.Domain.Imports;

public sealed record ImportError(
    Guid? Guid,
    string? Method,
    string? Project,
    string? Site,
    string Message
);
