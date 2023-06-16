using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.CommentEvents;

public abstract class CommentEvent : DomainEvent
{
    protected CommentEvent(string displayName, Comment comment) : base(displayName) => Comment = comment;

    public Comment Comment { get; }
}
