using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.CommentEvents;

public abstract class CommentEvent : IDomainEvent
{
    protected CommentEvent(Comment comment) => Comment = comment;

    public Comment Comment { get; }
}
