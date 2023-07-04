using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemEvents;

public class PunchItemDeletedEvent : PunchItemEvent
{
    public PunchItemDeletedEvent(PunchItem punchItem) : base(punchItem)
    { }
}
