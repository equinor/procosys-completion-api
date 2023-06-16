using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;

public class PunchUnvoidedEvent : PunchEvent
{
    public PunchUnvoidedEvent(Punch punch) : base("Punch unvoided", punch)
    { }
}
