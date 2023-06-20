using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;

public class PunchCreatedEvent : PunchEvent
{
    public PunchCreatedEvent(Punch punch, Guid projectGuid) : base(punch)
        => ProjectGuid = projectGuid;

    public Guid ProjectGuid { get; }
}
