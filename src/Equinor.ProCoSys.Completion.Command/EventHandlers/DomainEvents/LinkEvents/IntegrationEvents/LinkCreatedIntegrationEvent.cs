using System;
using Equinor.ProCoSys.Completion.Domain.Events;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.Link;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.LinkEvents.IntegrationEvents;

public record LinkCreatedIntegrationEvent
(
    string DisplayName,
    Guid Guid,
    Guid ParentGuid,
    string ParentType,
    string Title,
    string Url,
    IUser CreatedBy,
    DateTime CreatedAtUtc
) : ILinkCreatedV1
{
    internal LinkCreatedIntegrationEvent(LinkCreatedDomainEvent domainEvent) : this(
        $"Link {domainEvent.Link.Title} created",
        domainEvent.Link.Guid,
        domainEvent.Link.ParentGuid,
        domainEvent.Link.ParentType,
        domainEvent.Link.Title,
        domainEvent.Link.Url,
        new User(domainEvent.Link.CreatedBy.Guid, domainEvent.Link.CreatedBy.GetFullName()),
        domainEvent.Link.CreatedAtUtc)
    { }
}
