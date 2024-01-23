using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.Attachment;
using Equinor.ProCoSys.Completion.MessageContracts.History;

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
    DateTime ModifiedAtUtc,
    List<IChangedProperty> Changes
) : IAttachmentUpdatedV1
{
    internal AttachmentUpdatedIntegrationEvent(ExistingAttachmentUploadedAndOverwrittenDomainEvent domainEvent) : this(
        $"Attachment {domainEvent.Attachment.FileName} uploaded again",
        domainEvent.Attachment,
        new List<IChangedProperty>())
    { }

    internal AttachmentUpdatedIntegrationEvent(AttachmentUpdatedDomainEvent domainEvent) : this(
        $"Attachment {domainEvent.Attachment.FileName} updated",
        domainEvent.Attachment,
        domainEvent.Changes)
    { }

    private AttachmentUpdatedIntegrationEvent(string displayName, Attachment attachment, List<IChangedProperty> changes) : this(
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
        attachment.ModifiedAtUtc!.Value,
        changes)
    { }
}
