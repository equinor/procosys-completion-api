using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.CommentDomainEvents;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.CommentEvents;

public class CommentCreatedEventHandler : INotificationHandler<CommentCreatedDomainEvent>
{
    public Task Handle(CommentCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var sourceGuid = notification.Comment.SourceGuid;

        // ToDo #104081 Publish message
        return Task.CompletedTask;
    }
}
