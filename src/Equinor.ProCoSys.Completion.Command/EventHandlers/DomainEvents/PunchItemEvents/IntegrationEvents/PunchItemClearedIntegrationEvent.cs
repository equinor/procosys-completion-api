using System;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;

public record PunchItemClearedIntegrationEvent
(
    string DisplayName,
    Guid Guid,
    Guid ClearedByOid,
    DateTime ClearedAtUtc,
    Guid ModifiedByOid,
    DateTime ModifiedAtUtc
) : IPunchItemClearedV1
{
    internal PunchItemClearedIntegrationEvent(
        PunchItemClearedDomainEvent punchItemClearedEvent) : this(
        DisplayName: "Punch item cleared",
        punchItemClearedEvent.PunchItem.Guid,
        punchItemClearedEvent.PunchItem.ClearedBy!.Guid,
        punchItemClearedEvent.PunchItem.ClearedAtUtc!.Value,
        punchItemClearedEvent.PunchItem.ModifiedBy!.Guid,
        punchItemClearedEvent.PunchItem.ModifiedAtUtc!.Value)
    { }
}
