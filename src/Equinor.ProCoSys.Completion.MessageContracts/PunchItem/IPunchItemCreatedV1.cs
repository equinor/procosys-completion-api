using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.PunchItem;

public interface IPunchItemCreatedV1 : IPunchItem, IIntegrationEvent
{
    // Guid of the entity owning the Punch
    Guid ParentGuid { get; }
}
