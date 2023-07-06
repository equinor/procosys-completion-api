using System;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;

public record PunchItemCreatedIntegrationEvent
(
    string DisplayName,
    Guid ProjectGuid,
    Guid Guid,
    string ItemNo,
    Guid CreatedByOid,
    DateTime CreatedAtUtc
) : IPunchItemCreatedV1
{
    internal PunchItemCreatedIntegrationEvent(PunchItemCreatedDomainEvent punchItemCreatedEvent) : this(
        DisplayName: "Punch item created",
        punchItemCreatedEvent.ProjectGuid,
        punchItemCreatedEvent.PunchItem.Guid,
        punchItemCreatedEvent.PunchItem.ItemNo,
        punchItemCreatedEvent.PunchItem.CreatedByOid,
        punchItemCreatedEvent.PunchItem.CreatedAtUtc)
    { }
}
