using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentEvents;

public abstract class AttachmentEvent : IDomainEvent
{
    protected AttachmentEvent(Attachment attachment) => Attachment = attachment;

    public Attachment Attachment { get; }
}
