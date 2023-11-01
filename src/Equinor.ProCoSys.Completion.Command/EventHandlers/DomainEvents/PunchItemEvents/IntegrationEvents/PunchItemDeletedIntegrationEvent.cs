using System;
using Equinor.ProCoSys.Completion.Domain.Events;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.PunchItem;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;

public record PunchItemDeletedIntegrationEvent
(
    string DisplayName,
    Guid Guid,
    IUser DeletedBy,
    DateTime DeletedAtUtc
) : IPunchItemDeletedV1
{
    internal PunchItemDeletedIntegrationEvent(PunchItemDeletedDomainEvent domainEvent) : this(
        "Punch item deleted",
        domainEvent.PunchItem.Guid,
        // Our entities don't have DeletedByOid / DeletedAtUtc ...
        // ... but both ModifiedBy and ModifiedAtUtc are updated when entity is deleted
        new User(domainEvent.PunchItem.ModifiedBy!.Guid, domainEvent.PunchItem.ModifiedBy!.GetFullName()),
        domainEvent.PunchItem.ModifiedAtUtc!.Value)
    { }
}
