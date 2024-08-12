using System;
using Equinor.ProCoSys.Completion.MessageContracts;
using MassTransit;

namespace Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.AttachmentEvents;

public record AttachmentDeletedByPunchItemIntegrationEvent(Guid Guid, string FullBlobPath) : IIntegrationEvent
{
     public Guid MessageId { get; }  = NewId.NextGuid();
}
