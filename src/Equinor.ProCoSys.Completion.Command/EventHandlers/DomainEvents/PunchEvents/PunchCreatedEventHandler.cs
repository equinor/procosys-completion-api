using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchEvents;

public class PunchCreatedEventHandler : INotificationHandler<PunchCreatedEvent>
{
    // todo unit test
    public Task Handle(PunchCreatedEvent notification, CancellationToken cancellationToken)
    {
        var sourceGuid = notification.Punch.Guid;

        // ToDo Send event to the bus
        return Task.CompletedTask;
    }
}
