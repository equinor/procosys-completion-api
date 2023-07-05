using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemEvents;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents;

public class PunchItemUpdatedEventHandler : INotificationHandler<PunchItemUpdatedEvent>
{
    public Task Handle(PunchItemUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var sourceGuid = notification.PunchItem.Guid;

        // ToDo #104081 Publish message
        return Task.CompletedTask;
    }
}
