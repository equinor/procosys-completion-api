using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentDomainEvents;

public class AttachmentDeletedDomainEvent : AttachmentDomainEvent
{
    public AttachmentDeletedDomainEvent(Attachment attachment) : base(attachment)
    {
    }
}
