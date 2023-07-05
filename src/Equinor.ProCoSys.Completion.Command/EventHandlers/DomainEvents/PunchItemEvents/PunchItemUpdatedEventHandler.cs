using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents;

public class PunchItemUpdatedEventHandler : INotificationHandler<PunchItemUpdatedDomainEvent>
{
    public Task Handle(PunchItemUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var sourceGuid = notification.PunchItem.Guid;

        // ToDo #104081 Publish message
        return Task.CompletedTask;
    }
}
