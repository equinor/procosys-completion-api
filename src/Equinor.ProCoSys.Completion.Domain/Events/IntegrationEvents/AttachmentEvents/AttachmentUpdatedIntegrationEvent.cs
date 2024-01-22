using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.Attachment;

namespace Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.AttachmentEvents;

public record AttachmentUpdatedIntegrationEvent
(
    string Plant,
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
    public AttachmentUpdatedIntegrationEvent(Attachment attachment, string plant) : this(
        plant,
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
