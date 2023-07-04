using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemEvents;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents;

public class PunchItemDeletedEventHandler : INotificationHandler<PunchItemDeletedEvent>
{
    public Task Handle(PunchItemDeletedEvent notification, CancellationToken cancellationToken)
    {
        var sourceGuid = notification.PunchItem.Guid;

        // ToDo #104081 Publish message
        return Task.CompletedTask;
    }
}
