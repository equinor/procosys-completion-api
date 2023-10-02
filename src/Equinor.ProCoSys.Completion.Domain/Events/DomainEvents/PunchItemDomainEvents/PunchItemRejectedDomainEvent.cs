using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;

public class PunchItemRejectedDomainEvent : PunchItemDomainEvent
{
    public PunchItemRejectedDomainEvent(PunchItem punchItem) : base(punchItem)
    { }
}
