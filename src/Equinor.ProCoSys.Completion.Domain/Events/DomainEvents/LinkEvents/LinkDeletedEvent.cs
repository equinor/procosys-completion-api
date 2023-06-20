using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkEvents;

public class LinkDeletedEvent : LinkEvent
{
    public LinkDeletedEvent(Link link) : base(link)
    {
    }
}
