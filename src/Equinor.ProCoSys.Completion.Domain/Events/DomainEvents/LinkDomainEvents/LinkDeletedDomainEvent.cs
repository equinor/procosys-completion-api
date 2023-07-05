using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkDomainEvents;

public class LinkDeletedDomainEvent : LinkDomainEvent
{
    public LinkDeletedDomainEvent(Link link) : base(link)
    {
    }
}
