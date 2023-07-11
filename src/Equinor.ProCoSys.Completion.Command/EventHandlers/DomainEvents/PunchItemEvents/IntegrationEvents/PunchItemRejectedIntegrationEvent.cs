using System;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;

public record PunchItemRejectedIntegrationEvent
(
    string DisplayName,
    Guid Guid,
    Guid RejectedByOid,
    DateTime RejectedAtUtc,
    Guid ModifiedByOid,
    DateTime ModifiedAtUtc
) : IPunchItemRejectedV1
{
    internal PunchItemRejectedIntegrationEvent(
        PunchItemRejectedDomainEvent punchItemRejectedEvent) : this(
        DisplayName: "Punch item rejected",
        punchItemRejectedEvent.PunchItem.Guid,
        punchItemRejectedEvent.RejectedByOid,
        punchItemRejectedEvent.PunchItem.RejectedAtUtc!.Value,
        punchItemRejectedEvent.PunchItem.ModifiedByOid!.Value,
        punchItemRejectedEvent.PunchItem.ModifiedAtUtc!.Value)
    { }
}
