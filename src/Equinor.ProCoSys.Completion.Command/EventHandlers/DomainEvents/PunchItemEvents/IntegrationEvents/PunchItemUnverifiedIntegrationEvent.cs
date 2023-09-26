using System;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;

public record PunchItemUnverifiedIntegrationEvent
(
    string DisplayName,
    Guid Guid,
    Guid ModifiedByOid,
    DateTime ModifiedAtUtc
) : IPunchItemUnverifiedV1
{
    internal PunchItemUnverifiedIntegrationEvent(
        PunchItemUnverifiedDomainEvent punchItemUnverifiedEvent) : this(
        DisplayName: "Punch item unverified",
        punchItemUnverifiedEvent.PunchItem.Guid,
        punchItemUnverifiedEvent.PunchItem.ModifiedBy!.Guid,
        punchItemUnverifiedEvent.PunchItem.ModifiedAtUtc!.Value)
    { }
}
