using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.CommentEvents;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.CommentEvents;

public class CommentCreatedEventHandler : INotificationHandler<CommentCreatedEvent>
{
    // todo unit test
    public Task Handle(CommentCreatedEvent notification, CancellationToken cancellationToken)
    {
        var sourceGuid = notification.Comment.SourceGuid;

        // ToDo Send event to the bus
        return Task.CompletedTask;
    }
}
