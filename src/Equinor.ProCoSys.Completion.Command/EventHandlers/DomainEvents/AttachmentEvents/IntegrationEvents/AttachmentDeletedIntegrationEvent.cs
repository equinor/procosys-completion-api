using System;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts.Attachment;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.AttachmentEvents.IntegrationEvents;

public record AttachmentDeletedIntegrationEvent
(
    string DisplayName,
    Guid Guid,
    Guid SourceGuid,
    Guid DeletedByOid,
    DateTime DeletedAtUtc
) : IAttachmentDeletedV1
{
    internal AttachmentDeletedIntegrationEvent(AttachmentDeletedDomainEvent punchItemDomainEvent) : this(
        "Attachment deleted",
        punchItemDomainEvent.Attachment.Guid,
        punchItemDomainEvent.Attachment.SourceGuid,
        // Our entities don't have DeletedByOid / DeletedAtUtc ...
        // ... but both ModifiedBy and ModifiedAtUtc are updated when entity is deleted
        punchItemDomainEvent.Attachment.ModifiedBy!.Guid,
        punchItemDomainEvent.Attachment.ModifiedAtUtc!.Value)
    { }
}
