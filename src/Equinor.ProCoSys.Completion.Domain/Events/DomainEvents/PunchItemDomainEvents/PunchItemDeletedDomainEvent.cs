using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;

public class PunchItemDeletedDomainEvent : PunchItemDomainEvent
{
    public PunchItemDeletedDomainEvent(PunchItem punchItem) : base(punchItem, null)
    { }
}
