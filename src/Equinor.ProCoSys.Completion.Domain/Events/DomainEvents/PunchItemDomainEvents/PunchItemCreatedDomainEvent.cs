using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;

public class PunchItemCreatedDomainEvent : PunchItemDomainEvent
{
    public PunchItemCreatedDomainEvent(PunchItem punchItem) : base(punchItem)
    { }
}
