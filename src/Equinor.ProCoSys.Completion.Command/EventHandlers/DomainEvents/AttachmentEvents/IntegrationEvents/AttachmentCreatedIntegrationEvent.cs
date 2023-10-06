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
    int RevisionNumber,
    Guid CreatedByOid,
    DateTime CreatedAtUtc
) : IAttachmentCreatedV1
{
    internal AttachmentCreatedIntegrationEvent(NewAttachmentUploadedDomainEvent attachmentCreatedEvent) : this(
        "Attachment uploaded",
        attachmentCreatedEvent.Attachment)
    { }

    internal AttachmentCreatedIntegrationEvent(ExistingAttachmentUploadedAndOverwrittenDomainEvent attachmentCreatedEvent) : this(
        "Attachment uploaded - new revision",
        attachmentCreatedEvent.Attachment)
    { }

    private AttachmentCreatedIntegrationEvent(string displayName, Attachment attachment) : this(
        displayName,
        attachment.Guid,
        attachment.SourceGuid,
        attachment.SourceType,
        attachment.FileName,
        attachment.BlobPath,
        attachment.RevisionNumber,
        attachment.CreatedBy.Guid,
        attachment.CreatedAtUtc)
    { }
}
