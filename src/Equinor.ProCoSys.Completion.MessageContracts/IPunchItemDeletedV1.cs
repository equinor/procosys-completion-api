using System;

namespace Equinor.ProCoSys.Completion.MessageContracts;

public interface IPunchItemDeletedV1 : IIntegrationEvent
{
    Guid DeletedByOid { get; }
    DateTime DeletedAtUtc { get;  }
}
