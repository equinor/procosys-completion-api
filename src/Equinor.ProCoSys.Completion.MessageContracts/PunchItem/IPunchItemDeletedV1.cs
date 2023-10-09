using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.PunchItem;

public interface IPunchItemDeletedV1 : IIntegrationEvent
{
    Guid DeletedByOid { get; }
    DateTime DeletedAtUtc { get;  }
}
