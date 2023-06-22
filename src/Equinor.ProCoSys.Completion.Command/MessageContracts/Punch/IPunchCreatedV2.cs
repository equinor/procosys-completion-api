using System;

namespace Equinor.ProCoSys.Completion.Command.MessageContracts.Punch;

public interface IPunchCreatedV2 : IHaveDisplayName, IEventMessage
{
    public Guid ProjectGuid { get; }
    public Guid Guid { get; init; }
    public string ItemNo { get; init; }
    public Guid CreatedByOid { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
