using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentEvents;

public class AttachmentDeletedEvent : AttachmentEvent
{
    public AttachmentDeletedEvent(Attachment attachment) : base(attachment)
    {
    }
}
