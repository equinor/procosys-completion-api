using System;
using Equinor.ProCoSys.Completion.Domain.Events;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.Attachment;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.AttachmentEvents.IntegrationEvents;

public record AttachmentCreatedIntegrationEvent
(
    string DisplayName,
    Guid Guid,
    Guid ParentGuid,
    string ParentType,
    string FileName,
    string BlobPath,
    IUser CreatedBy,
    DateTime CreatedAtUtc
) : IAttachmentCreatedV1
{
    internal AttachmentCreatedIntegrationEvent(NewAttachmentUploadedDomainEvent domainEvent) : this(
        $"Attachment {domainEvent.Attachment.FileName} uploaded",
        domainEvent.Attachment.Guid,
        domainEvent.Attachment.ParentGuid,
        domainEvent.Attachment.ParentType,
        domainEvent.Attachment.FileName,
        domainEvent.Attachment.BlobPath,
        new User(domainEvent.Attachment.CreatedBy.Guid, domainEvent.Attachment.CreatedBy.GetFullName()),
        domainEvent.Attachment.CreatedAtUtc)
    { }
}
