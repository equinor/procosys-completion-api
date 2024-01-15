using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.History;

public interface IHistoryItemDeletedV1 : IHistoryItem, IIntegrationEvent
{
    // Guid of the parent entity of the event
    Guid? ParentGuid { get; }
}
