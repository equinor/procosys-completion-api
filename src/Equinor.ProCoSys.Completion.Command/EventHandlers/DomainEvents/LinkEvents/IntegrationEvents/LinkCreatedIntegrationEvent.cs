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
    internal LinkCreatedIntegrationEvent(LinkCreatedDomainEvent linkCreatedEvent) : this(
        "Link created",
        linkCreatedEvent.Link.Guid,
        linkCreatedEvent.Link.SourceGuid,
        linkCreatedEvent.Link.SourceType,
        linkCreatedEvent.Link.Title,
        linkCreatedEvent.Link.Url,
        linkCreatedEvent.Link.CreatedBy.Guid,
        linkCreatedEvent.Link.CreatedAtUtc)
    { }
}
