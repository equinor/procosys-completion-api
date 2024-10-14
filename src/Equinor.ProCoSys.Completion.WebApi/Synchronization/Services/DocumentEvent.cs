using System;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization.Services;

public record DocumentEvent(
    string Plant,
    Guid ProCoSysGuid,
    string DocumentNo,
    bool IsVoided,
    DateTime LastUpdated,
    string? Behavior
);//: using fields from IDocumentEventV1;
