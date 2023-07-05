using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentEvents;

public class NewAttachmentUploadedEvent : AttachmentEvent
{
    public NewAttachmentUploadedEvent(Attachment attachment) : base(attachment)
    {
    }
}
