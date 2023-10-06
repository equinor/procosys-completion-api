using System;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts.Link;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.LinkEvents.IntegrationEvents;

public record LinkDeletedIntegrationEvent
(
    string DisplayName,
    Guid Guid,
    Guid SourceGuid,
    Guid DeletedByOid,
    DateTime DeletedAtUtc
) : ILinkDeletedV1
{
    internal LinkDeletedIntegrationEvent(LinkDeletedDomainEvent domainEvent) : this(
        $"Link {domainEvent.Link.Title} deleted",
        domainEvent.Link.Guid,
        domainEvent.Link.SourceGuid,
        // Our entities don't have DeletedByOid / DeletedAtUtc ...
        // ... but both ModifiedBy and ModifiedAtUtc are updated when entity is deleted
        domainEvent.Link.ModifiedBy!.Guid,
        domainEvent.Link.ModifiedAtUtc!.Value)
    { }
}
