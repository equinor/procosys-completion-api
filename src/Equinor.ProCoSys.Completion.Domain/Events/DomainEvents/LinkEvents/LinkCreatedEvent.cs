using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkEvents;

public class LinkCreatedEvent : LinkEvent
{
    public LinkCreatedEvent(Link link) : base(link)
    {
    }
}
