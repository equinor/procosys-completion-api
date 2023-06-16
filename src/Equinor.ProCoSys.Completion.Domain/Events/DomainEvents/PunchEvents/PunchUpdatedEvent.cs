using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;

public class PunchUpdatedEvent : PunchEvent
{
    public PunchUpdatedEvent(Punch punch) : base("Punch updated", punch)
    { }
}
