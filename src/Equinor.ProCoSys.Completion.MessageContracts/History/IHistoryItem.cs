using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.History;

public interface IHistoryItem
{
    string Plant { get; }
    // A human-readable name of the published event
    string DisplayName { get; }
    User EventBy { get; }
    DateTime EventAtUtc { get; }
}
