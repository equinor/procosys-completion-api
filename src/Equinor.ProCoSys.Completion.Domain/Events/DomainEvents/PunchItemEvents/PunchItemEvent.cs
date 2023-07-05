using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemEvents;

public abstract class PunchItemEvent : IDomainEvent
{
    protected PunchItemEvent(PunchItem punchItem) => PunchItem = punchItem;

    public PunchItem PunchItem { get; }
}
