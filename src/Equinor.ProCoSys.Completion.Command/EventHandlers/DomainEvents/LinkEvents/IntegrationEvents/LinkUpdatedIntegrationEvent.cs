using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.Link;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.LinkEvents.IntegrationEvents;

public record LinkUpdatedIntegrationEvent
(
    string DisplayName,
    Guid Guid,
    Guid SourceGuid,
    string SourceType,
    string Title,
    string Url,
    Guid ModifiedByOid,
    DateTime ModifiedAtUtc,
    List<IProperty> Changes
) : ILinkUpdatedV1
{
    internal LinkUpdatedIntegrationEvent(LinkUpdatedDomainEvent domainEvent) : this(
        $"Link {domainEvent.Link.Title} updated",
        domainEvent.Link.Guid,
        domainEvent.Link.SourceGuid,
        domainEvent.Link.SourceType,
        domainEvent.Link.Title,
        domainEvent.Link.Url,
        domainEvent.Link.ModifiedBy!.Guid,
        domainEvent.Link.ModifiedAtUtc!.Value,
        domainEvent.Changes)
    { }
}
