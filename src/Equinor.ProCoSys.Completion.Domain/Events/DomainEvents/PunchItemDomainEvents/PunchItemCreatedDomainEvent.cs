using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;

public class PunchItemCreatedDomainEvent : PunchItemDomainEvent
{
    // ToDo #104017 extend with Guids for Library-values such as PunchPriority etc #.
    public PunchItemCreatedDomainEvent(PunchItem punchItem, Guid projectGuid) : base(punchItem)
        => ProjectGuid = projectGuid;

    public Guid ProjectGuid { get; }
}
