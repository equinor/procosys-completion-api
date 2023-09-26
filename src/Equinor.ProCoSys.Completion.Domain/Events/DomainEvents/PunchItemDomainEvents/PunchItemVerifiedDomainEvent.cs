using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;

public class PunchItemVerifiedDomainEvent : PunchItemDomainEvent
{
    public PunchItemVerifiedDomainEvent(PunchItem punchItem) : base(punchItem)
    {
    }
}
