using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentEvents;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.AttachmentEvents;

public class AttachmentDeletedEventHandler : INotificationHandler<AttachmentDeletedEvent>
{
    // todo unit test
    public Task Handle(AttachmentDeletedEvent notification, CancellationToken cancellationToken)
    {
        var sourceGuid = notification.Attachment.SourceGuid;

        // ToDo Send event to the bus
        return Task.CompletedTask;
    }
}
