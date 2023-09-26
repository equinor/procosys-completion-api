using System;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;

public record PunchItemUnclearedIntegrationEvent
(
    string DisplayName,
    Guid Guid,
    Guid ModifiedByOid,
    DateTime ModifiedAtUtc
) : IPunchItemUnclearedV1
{
    internal PunchItemUnclearedIntegrationEvent(
        PunchItemUnclearedDomainEvent punchItemUnclearedEvent) : this(
        DisplayName: "Punch item uncleared",
        punchItemUnclearedEvent.PunchItem.Guid,
        punchItemUnclearedEvent.PunchItem.ModifiedBy!.Guid,
        punchItemUnclearedEvent.PunchItem.ModifiedAtUtc!.Value)
    { }
}
