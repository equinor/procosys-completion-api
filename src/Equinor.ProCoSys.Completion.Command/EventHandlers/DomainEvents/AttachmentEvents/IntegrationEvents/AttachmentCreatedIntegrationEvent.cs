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
    Guid SourceGuid,
    string SourceType,
    string FileName,
    string BlobPath,
    IUser CreatedBy,
    DateTime CreatedAtUtc
) : IAttachmentCreatedV1
{
    internal AttachmentCreatedIntegrationEvent(NewAttachmentUploadedDomainEvent domainEvent) : this(
        $"Attachment {domainEvent.Attachment.FileName} uploaded",
        domainEvent.Attachment.Guid,
        domainEvent.Attachment.SourceGuid,
        domainEvent.Attachment.SourceType,
        domainEvent.Attachment.FileName,
        domainEvent.Attachment.BlobPath,
        new User(domainEvent.Attachment.CreatedBy.Guid, domainEvent.Attachment.CreatedBy.GetFullName()),
        domainEvent.Attachment.CreatedAtUtc)
    { }
}
