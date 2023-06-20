using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkEvents;

public abstract class LinkEvent : IDomainEvent
{
    protected LinkEvent(Link link) => Link = link;

    public Link Link { get; }
}
