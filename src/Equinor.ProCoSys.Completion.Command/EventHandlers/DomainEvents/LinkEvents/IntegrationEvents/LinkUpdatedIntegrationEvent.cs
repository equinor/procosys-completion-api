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
    internal LinkUpdatedIntegrationEvent(LinkUpdatedDomainEvent punchItemDomainEvent) : this(
        "Link updated",
        punchItemDomainEvent.Link.Guid,
        punchItemDomainEvent.Link.SourceGuid,
        punchItemDomainEvent.Link.SourceType,
        punchItemDomainEvent.Link.Title,
        punchItemDomainEvent.Link.Url,
        punchItemDomainEvent.Link.ModifiedBy!.Guid,
        punchItemDomainEvent.Link.ModifiedAtUtc!.Value,
        punchItemDomainEvent.Changes)
    { }
}
