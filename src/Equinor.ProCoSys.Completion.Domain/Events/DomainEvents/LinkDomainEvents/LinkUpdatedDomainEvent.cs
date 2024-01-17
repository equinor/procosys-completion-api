using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.MessageContracts.History;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkDomainEvents;

public class LinkUpdatedDomainEvent : LinkDomainEvent
{
    public LinkUpdatedDomainEvent(Link link, List<IProperty> changes) : base(link)
        => Changes = changes;

    public List<IProperty> Changes { get; }
}
