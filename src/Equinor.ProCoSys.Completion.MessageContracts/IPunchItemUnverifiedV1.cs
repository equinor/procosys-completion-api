using System;

namespace Equinor.ProCoSys.Completion.MessageContracts;

public interface IPunchItemUnverifiedV1 : IHaveDisplayName, IIntegrationEvent
{
    public Guid Guid { get; init; }
    public Guid ModifiedByOid { get; init; }
    public DateTime ModifiedAtUtc { get; init; }
}
