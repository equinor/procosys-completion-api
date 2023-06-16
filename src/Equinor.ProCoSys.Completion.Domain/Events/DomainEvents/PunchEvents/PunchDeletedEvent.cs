using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;

public class PunchDeletedEvent : PunchEvent
{
    public PunchDeletedEvent(Punch punch) : base("Punch deleted", punch)
    { }
}
