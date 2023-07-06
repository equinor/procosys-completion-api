using System;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;

public record PunchItemVerifiedIntegrationEvent
(
    string DisplayName,
    Guid Guid,
    Guid VerifiedByOid,
    DateTime VerifiedAtUtc,
    Guid ModifiedByOid,
    DateTime ModifiedAtUtc
) : IPunchItemVerifiedV1
{
    internal PunchItemVerifiedIntegrationEvent(
        PunchItemVerifiedDomainEvent punchItemVerifiedEvent) : this(
        DisplayName: "Punch item verified",
        punchItemVerifiedEvent.PunchItem.Guid,
        punchItemVerifiedEvent.VerifiedByOid,
        punchItemVerifiedEvent.PunchItem.VerifiedAtUtc!.Value,
        punchItemVerifiedEvent.PunchItem.ModifiedByOid!.Value,
        punchItemVerifiedEvent.PunchItem.ModifiedAtUtc!.Value)
    { }
}
