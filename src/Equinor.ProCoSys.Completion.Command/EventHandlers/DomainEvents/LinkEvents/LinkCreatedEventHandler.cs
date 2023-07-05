using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkEvents;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.LinkEvents;

public class CommentCreatedEventHandler : INotificationHandler<LinkCreatedEvent>
{
    public Task Handle(LinkCreatedEvent notification, CancellationToken cancellationToken)
    {
        var sourceGuid = notification.Link.SourceGuid;

        // ToDo #104081 Publish message
        return Task.CompletedTask;
    }
}
