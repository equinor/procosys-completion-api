using System;

namespace Equinor.ProCoSys.Completion.MessageContracts;

public interface IPunchItemClearedV1 : IHaveDisplayName, IIntegrationEvent
{
    public Guid Guid { get; init; }
    public Guid ModifiedByOid { get; init; }
    public DateTime ModifiedAtUtc { get; init; }
    public Guid ClearedByOid { get; init; }
    public DateTime ClearedAtUtc { get; init; }
}
