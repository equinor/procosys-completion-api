using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;

public class PunchCreatedEvent : PunchEvent
{
    // todo extend with Guids for Library-values such as PunchPriority etc #.
    public PunchCreatedEvent(Punch punch, Guid projectGuid) : base(punch)
        => ProjectGuid = projectGuid;

    public Guid ProjectGuid { get; }
}
