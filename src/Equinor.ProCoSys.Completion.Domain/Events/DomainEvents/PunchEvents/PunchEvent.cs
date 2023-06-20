using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;

public abstract class PunchEvent : IDomainEvent
{
    protected PunchEvent(Punch punch) => Punch = punch;

    public Punch Punch { get; }
}
