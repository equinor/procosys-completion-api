using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Equinor.ProCoSys.Completion.MessageContracts.Link;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.LinkEvents.IntegrationEvents;

public record LinkUpdatedIntegrationEvent
(
    string DisplayName,
    Guid Guid,
    Guid ParentGuid,
    string ParentType,
    string Title,
    string Url,
    User ModifiedBy,
    DateTime ModifiedAtUtc,
    List<IProperty> Changes
) : ILinkUpdatedV1
{
    internal LinkUpdatedIntegrationEvent(LinkUpdatedDomainEvent domainEvent) : this(
        $"Link {domainEvent.Link.Title} updated",
        domainEvent.Link.Guid,
        domainEvent.Link.ParentGuid,
        domainEvent.Link.ParentType,
        domainEvent.Link.Title,
        domainEvent.Link.Url,
        new User(domainEvent.Link.ModifiedBy!.Guid, domainEvent.Link.ModifiedBy!.GetFullName()),
        domainEvent.Link.ModifiedAtUtc!.Value,
        domainEvent.Changes)
    { }
}
