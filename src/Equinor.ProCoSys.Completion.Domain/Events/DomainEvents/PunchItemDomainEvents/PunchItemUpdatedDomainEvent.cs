using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;

public class PunchItemUpdatedDomainEvent : PunchItemDomainEvent
{
    public PunchItemUpdatedDomainEvent(PunchItem punchItem, List<Property>? properties) : base(punchItem, properties)
    { }
}
