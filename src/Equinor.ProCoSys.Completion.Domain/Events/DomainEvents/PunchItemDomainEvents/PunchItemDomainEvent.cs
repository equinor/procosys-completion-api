using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;

public abstract class PunchItemDomainEvent : IDomainEvent
{
    protected PunchItemDomainEvent(PunchItem punchItem) 
        => PunchItem = punchItem;

    public PunchItem PunchItem { get; }
}
