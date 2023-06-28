using System;
using Equinor.ProCoSys.Completion.Command.MessageContracts;

// ReSharper disable once CheckNamespace
namespace Equinor.ProCoSys.MessageContracts.Punch;

public interface IPunchCreatedV1 : IHaveDisplayName, IEventMessage
{
    public Guid ProjectGuid { get; }
    public Guid Guid { get;  }
    public string ItemNo { get; }
    public Guid CreatedByOid { get; }
    public DateTime CreatedAtUtc { get;  }
}
