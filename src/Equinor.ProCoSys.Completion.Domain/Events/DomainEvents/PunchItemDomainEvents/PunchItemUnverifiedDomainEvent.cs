using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;

public class PunchItemUnverifiedDomainEvent : PunchItemDomainEvent
{
    public PunchItemUnverifiedDomainEvent(PunchItem punchItem) : base(punchItem, null)
    { }
}
