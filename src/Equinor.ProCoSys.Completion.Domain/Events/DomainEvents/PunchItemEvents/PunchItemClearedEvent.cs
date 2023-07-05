using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemEvents;

public class PunchItemClearedEvent : PunchItemEvent
{
    public PunchItemClearedEvent(PunchItem punchItem, Guid clearedByOid) : base(punchItem)
        => ClearedByOid = clearedByOid;

    public Guid ClearedByOid { get; }
}
