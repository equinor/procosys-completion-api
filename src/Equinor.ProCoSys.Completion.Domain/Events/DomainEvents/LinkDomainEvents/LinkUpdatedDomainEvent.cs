using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkDomainEvents;

public class LinkUpdatedDomainEvent : LinkDomainEvent
{
    public LinkUpdatedDomainEvent(Link link) : base(link)
    {
    }
}
