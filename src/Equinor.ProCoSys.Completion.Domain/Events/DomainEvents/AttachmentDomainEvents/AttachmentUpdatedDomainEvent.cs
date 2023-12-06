using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentDomainEvents;

public class AttachmentUpdatedDomainEvent : AttachmentDomainEvent
{
    public AttachmentUpdatedDomainEvent(Attachment attachment) : base(attachment)
    {
    }
}
