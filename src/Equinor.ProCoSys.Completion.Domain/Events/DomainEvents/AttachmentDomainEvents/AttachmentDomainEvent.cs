using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentDomainEvents;

public abstract class AttachmentDomainEvent : IDomainEvent
{
    protected AttachmentDomainEvent(Attachment attachment) => Attachment = attachment;

    public Attachment Attachment { get; }
}
