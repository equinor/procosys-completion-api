using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.MessageContracts;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;

public class PunchItemRejectedDomainEvent : PunchItemDomainEvent
{
    public PunchItemRejectedDomainEvent(PunchItem punchItem, List<IProperty> changes) : base(punchItem)
        => Changes = changes;

    public List<IProperty> Changes { get; }
}
