using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.History;

public interface IHistoryItem
{
    // A human-readable name of the published event
    string DisplayName { get; }
    User EventBy { get; }
    DateTime EventAtUtc { get; }
}
