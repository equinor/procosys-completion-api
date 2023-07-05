using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkDomainEvents;

public abstract class LinkDomainEvent : IDomainEvent
{
    protected LinkDomainEvent(Link link) => Link = link;

    public Link Link { get; }
}
