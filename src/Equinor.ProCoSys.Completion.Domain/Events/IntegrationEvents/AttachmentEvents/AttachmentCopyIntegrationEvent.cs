using System;
using Equinor.ProCoSys.Completion.MessageContracts;
using MassTransit;

namespace Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.AttachmentEvents;

public record AttachmentCopyIntegrationEvent(Guid Guid, string SrcBlobPath, Guid DestGuid, string DestBlobPath)
    : IIntegrationEvent
{
    public Guid MessageId { get; } = NewId.NextGuid();
}
