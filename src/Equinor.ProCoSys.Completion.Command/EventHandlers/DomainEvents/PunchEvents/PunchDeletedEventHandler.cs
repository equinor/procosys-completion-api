using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchEvents;

public class PunchDeletedEventHandler : INotificationHandler<PunchDeletedEvent>
{
    public Task Handle(PunchDeletedEvent notification, CancellationToken cancellationToken)
    {
        var sourceGuid = notification.Punch.Guid;

        // ToDo #104081 Publish message
        return Task.CompletedTask;
    }
}
