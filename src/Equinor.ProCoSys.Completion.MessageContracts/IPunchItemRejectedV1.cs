using System;

namespace Equinor.ProCoSys.Completion.MessageContracts;

public interface IPunchItemRejectedV1 : IHaveDisplayName, IIntegrationEvent
{
    public Guid Guid { get; init; }
    // ModifiedByOid and RejectedByOid will always be equal, but we expose both since different consumers can have different interests
    public Guid ModifiedByOid { get; init; }
    // ModifiedAtUtc and RejectedAtUtc will always be equal, but we expose both since different consumers can have different interests
    public DateTime ModifiedAtUtc { get; init; }
    public Guid RejectedByOid { get; init; }
    public DateTime RejectedAtUtc { get; init; }
}
