using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.Attachment;
using MassTransit;

namespace Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.AttachmentEvents;

public record AttachmentCreatedIntegrationEvent
(
    string Plant,
    Guid Guid,
    Guid ParentGuid,
    string ParentType,
    string FileName,
    string BlobPath,
    User CreatedBy,
    DateTime CreatedAtUtc
) : IAttachmentCreatedV1
{
    public AttachmentCreatedIntegrationEvent(Attachment attachment, string plant) : this(
        plant,
        attachment.Guid,
        attachment.ParentGuid,
        attachment.ParentType,
        attachment.FileName,
        attachment.BlobPath,
        new User(attachment.CreatedBy.Guid, attachment.CreatedBy.GetFullName()),
        attachment.CreatedAtUtc)
    { }

    public Guid MessageId { get; }  = NewId.NextGuid();
}
