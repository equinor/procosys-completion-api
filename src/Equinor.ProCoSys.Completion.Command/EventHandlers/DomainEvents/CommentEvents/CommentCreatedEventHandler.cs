using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.CommentEvents;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.CommentEvents;

public class CommentCreatedEventHandler : INotificationHandler<CommentCreatedEvent>
{
    public Task Handle(CommentCreatedEvent notification, CancellationToken cancellationToken)
    {
        var sourceGuid = notification.Comment.SourceGuid;

        // ToDo #104081 Publish message
        return Task.CompletedTask;
    }
}
