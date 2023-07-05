using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentDomainEvents;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.AttachmentEvents;

public class AttachmentDeletedEventHandler : INotificationHandler<AttachmentDeletedDomainEvent>
{
    public Task Handle(AttachmentDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var sourceGuid = notification.Attachment.SourceGuid;

        // ToDo #104081 Publish message
        return Task.CompletedTask;
    }
}
