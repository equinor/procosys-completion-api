using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.PunchItem;

public interface IPunchItemDeletedV1 : IIntegrationEvent
{
    IUser DeletedBy { get; }
    DateTime DeletedAtUtc { get;  }
}
