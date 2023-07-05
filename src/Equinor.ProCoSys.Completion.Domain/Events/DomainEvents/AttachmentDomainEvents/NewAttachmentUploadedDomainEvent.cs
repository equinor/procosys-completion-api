using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentDomainEvents;

public class NewAttachmentUploadedDomainEvent : AttachmentDomainEvent
{
    public NewAttachmentUploadedDomainEvent(Attachment attachment) : base(attachment)
    {
    }
}
