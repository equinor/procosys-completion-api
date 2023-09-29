using System.Collections.Generic;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;

public class PunchItemDomainEvent : IDomainEvent
{
    protected PunchItemDomainEvent(PunchItem punchItem, List<Property>? properties)
    {
        PunchItem = punchItem;
        Properties = properties;
    }

    public PunchItem PunchItem { get; }
    public List<Property>? Properties { get; }
}
