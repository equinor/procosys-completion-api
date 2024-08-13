using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.Link;
using MassTransit;

namespace Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.LinkEvents;

public record LinkUpdatedIntegrationEvent
(
    string Plant,
    Guid Guid,
    Guid ParentGuid,
    string ParentType,
    string Title,
    string Url,
    User ModifiedBy,
    DateTime ModifiedAtUtc
) : ILinkUpdatedV1
{
    public LinkUpdatedIntegrationEvent(Link link, string plant) : this(
        plant,
        link.Guid,
        link.ParentGuid,
        link.ParentType,
        link.Title,
        link.Url,
        new User(link.ModifiedBy!.Guid, link.ModifiedBy!.GetFullName()),
        link.ModifiedAtUtc!.Value)
    { }

    public Guid MessageId { get; } = NewId.NextGuid();
}
