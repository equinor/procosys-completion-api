using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.History;

public interface IHistoryItem
{
    // Guid of the parent entity of the event (optional)
    // Should be set if the event should be available in the History of the parent
    // Sample: It can be appropriate that an update of an attachment is a part of
    // the history of the parent entity
    Guid? ParentGuid { get; }
    // A human-readable name of the published event
    string DisplayName { get; }
    User EventBy { get; }
    DateTime EventAtUtc { get; }
}
