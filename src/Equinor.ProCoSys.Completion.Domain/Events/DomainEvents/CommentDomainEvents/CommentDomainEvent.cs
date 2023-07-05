using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.CommentDomainEvents;

public abstract class CommentDomainEvent : IDomainEvent
{
    protected CommentDomainEvent(Comment comment) => Comment = comment;

    public Comment Comment { get; }
}
