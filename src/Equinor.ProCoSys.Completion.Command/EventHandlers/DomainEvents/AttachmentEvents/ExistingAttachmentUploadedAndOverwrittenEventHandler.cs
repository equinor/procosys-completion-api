using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentEvents;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.AttachmentEvents;

public class ExistingAttachmentUploadedAndOverwrittenEventHandler : INotificationHandler<ExistingAttachmentUploadedAndOverwrittenEvent>
{
    public Task Handle(ExistingAttachmentUploadedAndOverwrittenEvent notification, CancellationToken cancellationToken)
    {
        var sourceGuid = notification.Attachment.SourceGuid;

        // ToDo #104081 Publish message
        return Task.CompletedTask;
    }
}
