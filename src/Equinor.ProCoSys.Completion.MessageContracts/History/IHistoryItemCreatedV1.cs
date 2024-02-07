using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.MessageContracts.History;

public interface IHistoryItemCreatedV1 : IHistoryItem, IIntegrationEvent
{
    List<IProperty> Properties { get; }
}

