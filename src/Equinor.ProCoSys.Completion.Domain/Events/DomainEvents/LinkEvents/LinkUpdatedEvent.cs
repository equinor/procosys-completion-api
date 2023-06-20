using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkEvents;

public class LinkUpdatedEvent : LinkEvent
{
    public LinkUpdatedEvent(Link link) : base(link)
    {
    }
}
