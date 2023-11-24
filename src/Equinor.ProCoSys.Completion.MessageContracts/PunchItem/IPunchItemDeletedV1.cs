using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.PunchItem;

public interface IPunchItemDeletedV1 : IIntegrationEvent
{
    // Guid of the entity owning the Punch
    Guid ParentGuid { get; }
    User DeletedBy { get; }
    DateTime DeletedAtUtc { get;  }
}
