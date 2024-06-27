using System;
using Equinor.ProCoSys.Completion.MessageContracts;

namespace Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.AttachmentEvents;

public record AttachmentDeletedByPunchItemIntegrationEvent(Guid Guid, string FullBlobPath) : IIntegrationEvent;
