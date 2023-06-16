using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkEvents;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.LinkEvents;

public class LinkUpdatedEventHandler : INotificationHandler<LinkUpdatedEvent>
{
    // todo unit test
    public Task Handle(LinkUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var sourceGuid = notification.Link.SourceGuid;

        // ToDo Send event to the bus
        return Task.CompletedTask;
    }
}
