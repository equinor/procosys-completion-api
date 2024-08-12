using System;
using System.Text.Json.Serialization;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.Attachment;
using MassTransit;

namespace Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.AttachmentEvents;

[method: JsonConstructor]
public record AttachmentDeletedIntegrationEvent(
    string Plant,
    Guid Guid,
    Guid ParentGuid,
    string FullBlobPath,
    User DeletedBy,
    DateTime DeletedAtUtc)
    : IAttachmentDeletedV1
{
    public AttachmentDeletedIntegrationEvent(Attachment attachment, string plant) : this(
        plant,
        attachment.Guid,
        attachment.ParentGuid,
        attachment.GetFullBlobPath(),
        // Our entities don't have DeletedByOid / DeletedAtUtc ...
        // ... but both ModifiedBy and ModifiedAtUtc are updated when entity is deleted
        new User(attachment.ModifiedBy!.Guid, attachment.ModifiedBy!.GetFullName()),
        attachment.ModifiedAtUtc!.Value)
    { }

    public Guid MessageId { get; }  = NewId.NextGuid();
}
