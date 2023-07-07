using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;

public class PunchItemUnclearedDomainEvent : PunchItemDomainEvent
{
    public PunchItemUnclearedDomainEvent(PunchItem punchItem) : base(punchItem)
    { }
}
