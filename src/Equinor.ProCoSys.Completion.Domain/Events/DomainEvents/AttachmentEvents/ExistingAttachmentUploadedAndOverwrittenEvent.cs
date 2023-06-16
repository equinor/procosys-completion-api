using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentEvents;

public class ExistingAttachmentUploadedAndOverwrittenEvent : AttachmentEvent
{
    public ExistingAttachmentUploadedAndOverwrittenEvent(Attachment attachment)
        : base($"Existing attachment uploaded again for {attachment.SourceType}", attachment)
    {
    }
}
