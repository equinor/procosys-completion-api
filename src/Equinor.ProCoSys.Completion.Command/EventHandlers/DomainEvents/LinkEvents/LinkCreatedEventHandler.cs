using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkDomainEvents;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.LinkEvents;

public class CommentCreatedEventHandler : INotificationHandler<LinkCreatedDomainEvent>
{
    public Task Handle(LinkCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var sourceGuid = notification.Link.SourceGuid;

        // ToDo #104081 Publish message
        return Task.CompletedTask;
    }
}
