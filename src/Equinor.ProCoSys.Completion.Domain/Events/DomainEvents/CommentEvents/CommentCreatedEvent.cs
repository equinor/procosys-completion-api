using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.CommentEvents;

public class CommentCreatedEvent : CommentEvent
{
    public CommentCreatedEvent(Comment comment) : base(comment)
    {
    }
}
