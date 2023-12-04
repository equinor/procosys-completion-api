using System;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.Attachment;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.AttachmentEvents.IntegrationEvents;

public record AttachmentUpdatedIntegrationEvent
(
    string DisplayName,
    Guid Guid,
    Guid ParentGuid,
    string ParentType,
    string FileName,
    string BlobPath,
    User ModifiedBy,
    DateTime ModifiedAtUtc
) : IAttachmentUpdatedV1
{
    internal AttachmentUpdatedIntegrationEvent(ExistingAttachmentUploadedAndOverwrittenDomainEvent domainEvent) : this(
        $"Attachment {domainEvent.Attachment.FileName} uploaded again",
        domainEvent.Attachment.Guid,
        domainEvent.Attachment.ParentGuid,
        domainEvent.Attachment.ParentType,
        domainEvent.Attachment.FileName,
        domainEvent.Attachment.BlobPath,
        new User(domainEvent.Attachment.ModifiedBy!.Guid, domainEvent.Attachment.ModifiedBy!.GetFullName()),
        domainEvent.Attachment.ModifiedAtUtc!.Value)
    { }
}
