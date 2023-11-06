using System;
using Equinor.ProCoSys.Completion.Domain.Events;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.Attachment;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.AttachmentEvents.IntegrationEvents;

public record AttachmentDeletedIntegrationEvent
(
    string DisplayName,
    Guid Guid,
    Guid ParentGuid,
    IUser DeletedBy,
    DateTime DeletedAtUtc
) : IAttachmentDeletedV1
{
    internal AttachmentDeletedIntegrationEvent(AttachmentDeletedDomainEvent domainEvent) : this(
        $"Attachment {domainEvent.Attachment.FileName} deleted",
        domainEvent.Attachment.Guid,
        domainEvent.Attachment.SourceGuid,
        // Our entities don't have DeletedByOid / DeletedAtUtc ...
        // ... but both ModifiedBy and ModifiedAtUtc are updated when entity is deleted
        new User(domainEvent.Attachment.ModifiedBy!.Guid, domainEvent.Attachment.ModifiedBy!.GetFullName()),
        domainEvent.Attachment.ModifiedAtUtc!.Value)
    { }
}
