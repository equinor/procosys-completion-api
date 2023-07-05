using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemEvents;

public class PunchItemClearedEvent : PunchItemEvent
{
    private readonly Guid _clearedByOid;

    public PunchItemClearedEvent(PunchItem punchItem, Guid clearedByOid) : base(punchItem)
        => _clearedByOid = clearedByOid;
}
