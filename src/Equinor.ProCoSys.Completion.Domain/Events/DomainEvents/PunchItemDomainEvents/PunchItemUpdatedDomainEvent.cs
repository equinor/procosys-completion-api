using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;

public class PunchItemUpdatedDomainEvent : PunchItemDomainEvent
{
    public PunchItemUpdatedDomainEvent(PunchItem punchItem) : base(punchItem)
    { }
}
