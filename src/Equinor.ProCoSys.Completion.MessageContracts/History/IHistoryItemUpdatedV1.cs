﻿using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.MessageContracts.History;

public interface IHistoryItemUpdatedV1 : IHistoryItem, IIntegrationEvent
{
    List<IChangedProperty> ChangedProperties { get; }
}
