using System;

namespace Equinor.ProCoSys.Completion.MessageContracts;

public interface IPunchItemVerifiedV1 : IHaveDisplayName, IIntegrationEvent
{
    public Guid Guid { get; init; }
    // ModifiedByOid and VerifiedByOid will always be equal, but we expose both since different consumers can have different interests
    public Guid ModifiedByOid { get; init; }
    // ModifiedAtUtc and VerifiedAtUtc will always be equal, but we expose both since different consumers can have different interests
    public DateTime ModifiedAtUtc { get; init; }
    public Guid VerifiedByOid { get; init; }
    public DateTime VerifiedAtUtc { get; init; }
}
