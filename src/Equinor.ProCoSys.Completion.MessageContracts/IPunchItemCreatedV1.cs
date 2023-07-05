using System;

namespace Equinor.ProCoSys.Completion.MessageContracts;

public interface IPunchItemCreatedV1 : IHaveDisplayName, IEventMessage
{
    public Guid ProjectGuid { get; }
    public Guid Guid { get;  }
    public string ItemNo { get; }
    public Guid CreatedByOid { get; }
    public DateTime CreatedAtUtc { get;  }
}
