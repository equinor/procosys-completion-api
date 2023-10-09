using System;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts.Attachment;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.AttachmentEvents.IntegrationEvents;

public record AttachmentUpdatedIntegrationEvent
(
    string DisplayName,
    Guid Guid,
    Guid SourceGuid,
    string SourceType,
    string FileName,
    string BlobPath,
    Guid ModifiedByOid,
    DateTime ModifiedAtUtc
) : IAttachmentUpdatedV1
{
    internal AttachmentUpdatedIntegrationEvent(ExistingAttachmentUploadedAndOverwrittenDomainEvent domainEvent) : this(
        $"Attachment {domainEvent.Attachment.FileName} uploaded again",
        domainEvent.Attachment.Guid,
        domainEvent.Attachment.SourceGuid,
        domainEvent.Attachment.SourceType,
        domainEvent.Attachment.FileName,
        domainEvent.Attachment.BlobPath,
        domainEvent.Attachment.ModifiedBy!.Guid,
        domainEvent.Attachment.ModifiedAtUtc!.Value)
    { }
}
