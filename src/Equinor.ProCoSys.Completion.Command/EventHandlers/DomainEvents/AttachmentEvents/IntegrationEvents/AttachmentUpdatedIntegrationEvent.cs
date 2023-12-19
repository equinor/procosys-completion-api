using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
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
    string Description,
    string BlobPath,
    int RevisionNumber,
    List<string> Labels,
    User ModifiedBy,
    DateTime ModifiedAtUtc
) : IAttachmentUpdatedV1
{
    internal AttachmentUpdatedIntegrationEvent(ExistingAttachmentUploadedAndOverwrittenDomainEvent domainEvent) : this(
        $"Attachment {domainEvent.Attachment.FileName} uploaded again",
        domainEvent.Attachment)
    { }

    internal AttachmentUpdatedIntegrationEvent(AttachmentUpdatedDomainEvent domainEvent) : this(
        $"Attachment {domainEvent.Attachment.FileName} updated",
        domainEvent.Attachment)
    { }

    private AttachmentUpdatedIntegrationEvent(string displayName, Attachment attachment) : this(
        displayName,
        attachment.Guid,
        attachment.ParentGuid,
        attachment.ParentType,
        attachment.FileName,
        attachment.Description,
        attachment.BlobPath,
        attachment.RevisionNumber,
        attachment.GetOrderedNonVoidedLabels().Select(l => l.Text).ToList(),
        new User(attachment.ModifiedBy!.Guid, attachment.ModifiedBy!.GetFullName()),
        attachment.ModifiedAtUtc!.Value)
    { }
}
