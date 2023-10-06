using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentDomainEvents;
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
    Guid CreatedByOid,
    DateTime CreatedAtUtc
) : IAttachmentCreatedV1
{
    internal AttachmentCreatedIntegrationEvent(NewAttachmentUploadedDomainEvent domainEvent) : this(
        $"Attachment {domainEvent.Attachment.FileName} uploaded",
        domainEvent.Attachment)
    { }

    internal AttachmentCreatedIntegrationEvent(ExistingAttachmentUploadedAndOverwrittenDomainEvent domainEvent) : this(
        $"Attachment {domainEvent.Attachment.FileName} uploaded",
        domainEvent.Attachment)
    { }

    private AttachmentCreatedIntegrationEvent(string displayName, Attachment attachment) : this(
        displayName,
        attachment.Guid,
        attachment.SourceGuid,
        attachment.SourceType,
        attachment.FileName,
        attachment.BlobPath,
        attachment.CreatedBy.Guid,
        attachment.CreatedAtUtc)
    { }
}
