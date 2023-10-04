using System;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts.PunchItem;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;

public record PunchItemDeletedIntegrationEvent
(
    string DisplayName,
    Guid Guid,
    Guid DeletedByOid,
    DateTime DeletedAtUtc
) : IPunchItemDeletedV1
{
    internal PunchItemDeletedIntegrationEvent(PunchItemDeletedDomainEvent punchItemDeletedEvent) : this(
        DisplayName: "Punch item deleted",
        punchItemDeletedEvent.PunchItem.Guid,
        // Our entities don't have DeletedByOid / DeletedAtUtc ...
        // ... but both ModifiedBy and ModifiedAtUtc are updated when entity is deleted
        punchItemDeletedEvent.PunchItem.ModifiedBy!.Guid,
        punchItemDeletedEvent.PunchItem.ModifiedAtUtc!.Value)
    { }
}
