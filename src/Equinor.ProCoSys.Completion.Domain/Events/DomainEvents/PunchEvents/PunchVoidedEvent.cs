using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;

public class PunchVoidedEvent : PunchEvent
{
    public PunchVoidedEvent(Punch punch) : base(punch)
    { }
}
