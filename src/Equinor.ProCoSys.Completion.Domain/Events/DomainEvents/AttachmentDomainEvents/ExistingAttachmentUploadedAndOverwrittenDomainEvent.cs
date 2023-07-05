using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentDomainEvents;

public class ExistingAttachmentUploadedAndOverwrittenDomainEvent : AttachmentDomainEvent
{
    public ExistingAttachmentUploadedAndOverwrittenDomainEvent(Attachment attachment) : base(attachment)
    {
    }
}
