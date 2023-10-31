using System;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts.Link;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.LinkEvents.IntegrationEvents;

public record LinkCreatedIntegrationEvent
(
    string DisplayName,
    Guid Guid,
    Guid SourceGuid,
    string SourceType,
    string Title,
    string Url,
    Guid CreatedByOid,
    DateTime CreatedAtUtc
) : ILinkCreatedV1
{
    internal LinkCreatedIntegrationEvent(LinkCreatedDomainEvent domainEvent) : this(
        $"Link {domainEvent.Link.Title} created",
        domainEvent.Link.Guid,
        domainEvent.Link.SourceGuid,
        domainEvent.Link.SourceType,
        domainEvent.Link.Title,
        domainEvent.Link.Url,
        domainEvent.Link.CreatedBy.Guid,
        domainEvent.Link.CreatedAtUtc)
    { }
}
