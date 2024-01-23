using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.MessageContracts.History;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentDomainEvents;

public class AttachmentUpdatedDomainEvent : AttachmentDomainEvent
{
    public AttachmentUpdatedDomainEvent(Attachment attachment, List<IChangedProperty> changes) : base(attachment)
        => Changes = changes;

    public List<IChangedProperty> Changes { get; }
}
