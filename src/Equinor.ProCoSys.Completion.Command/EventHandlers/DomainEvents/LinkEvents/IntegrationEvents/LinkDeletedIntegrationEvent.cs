using System;
using Equinor.ProCoSys.Completion.Domain.Events;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.Link;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.LinkEvents.IntegrationEvents;

public record LinkDeletedIntegrationEvent
(
    string DisplayName,
    Guid Guid,
    Guid ParentGuid,
    IUser DeletedBy,
    DateTime DeletedAtUtc
) : ILinkDeletedV1
{
    internal LinkDeletedIntegrationEvent(LinkDeletedDomainEvent domainEvent) : this(
        $"Link {domainEvent.Link.Title} deleted",
        domainEvent.Link.Guid,
        domainEvent.Link.SourceGuid,
        // Our entities don't have DeletedByOid / DeletedAtUtc ...
        // ... but both ModifiedBy and ModifiedAtUtc are updated when entity is deleted
        new User(domainEvent.Link.ModifiedBy!.Guid, domainEvent.Link.ModifiedBy!.GetFullName()),
        domainEvent.Link.ModifiedAtUtc!.Value)
    { }
}
