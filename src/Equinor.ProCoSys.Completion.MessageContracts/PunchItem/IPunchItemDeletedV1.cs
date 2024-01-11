using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.PunchItem;

public interface IPunchItemDeletedV1 : IIntegrationEvent
{
    string Plant { get; }
    User DeletedBy { get; }
    DateTime DeletedAtUtc { get;  }
}
