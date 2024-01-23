using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.MessageContracts.History;

public interface IHistoryItemCreatedV1 : IHistoryItem, IIntegrationEvent
{
    // Guid of the parent entity of the event
    Guid? ParentGuid { get; }
    List<IProperty> Properties { get; }
}

