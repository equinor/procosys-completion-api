using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.Link;
using MassTransit;

namespace Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.LinkEvents;

public record LinkDeletedIntegrationEvent
(
    string Plant,
    Guid Guid,
    Guid ParentGuid,
    User DeletedBy,
    DateTime DeletedAtUtc
) : ILinkDeletedV1
{
    public LinkDeletedIntegrationEvent(Link link, string plant) : this(
        plant,
        link.Guid,
        link.ParentGuid,
        // Our entities don't have DeletedByOid / DeletedAtUtc ...
        // ... but both ModifiedBy and ModifiedAtUtc are updated when entity is deleted
        new User(link.ModifiedBy!.Guid, link.ModifiedBy!.GetFullName()),
        link.ModifiedAtUtc!.Value)
    { }

    public Guid MessageId { get; }  = NewId.NextGuid();
}
