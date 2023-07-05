using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkDomainEvents;

public class LinkCreatedDomainEvent : LinkDomainEvent
{
    public LinkCreatedDomainEvent(Link link) : base(link)
    {
    }
}
