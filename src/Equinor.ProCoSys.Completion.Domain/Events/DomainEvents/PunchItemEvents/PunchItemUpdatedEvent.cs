using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemEvents;

public class PunchItemUpdatedEvent : PunchItemEvent
{
    public PunchItemUpdatedEvent(PunchItem punchItem) : base(punchItem)
    { }
}
