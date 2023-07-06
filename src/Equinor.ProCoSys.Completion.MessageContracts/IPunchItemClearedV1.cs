using System;

namespace Equinor.ProCoSys.Completion.MessageContracts;

public interface IPunchItemClearedV1 : IHaveDisplayName, IIntegrationEvent
{
    public Guid Guid { get; init; }
    // ModifiedByOid and ClearedByOid will always be equal, but we expose both since different consumers can have different interests
    public Guid ModifiedByOid { get; init; }
    // ModifiedAtUtc and ClearedAtUtc will always be equal, but we expose both since different consumers can have different interests
    public DateTime ModifiedAtUtc { get; init; }
    public Guid ClearedByOid { get; init; }
    public DateTime ClearedAtUtc { get; init; }
}
