using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentEvents;

public abstract class AttachmentEvent : DomainEvent
{
    protected AttachmentEvent(string displayName, Attachment attachment) : base(displayName) => Attachment = attachment;

    public Attachment Attachment { get; }
}
