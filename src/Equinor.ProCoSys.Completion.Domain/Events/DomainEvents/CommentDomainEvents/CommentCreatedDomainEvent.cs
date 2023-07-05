using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.CommentDomainEvents;

public class CommentCreatedDomainEvent : CommentDomainEvent
{
    public CommentCreatedDomainEvent(Comment comment) : base(comment)
    {
    }
}
