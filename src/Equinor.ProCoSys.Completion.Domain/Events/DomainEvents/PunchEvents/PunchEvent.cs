using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;

public abstract class PunchEvent : DomainEvent
{
    protected PunchEvent(string displayName, Punch punch) : base(displayName) => Punch = punch;

    public Punch Punch { get; }
}
