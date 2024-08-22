using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.Link;
using MassTransit;

namespace Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.LinkEvents;

public record LinkCreatedIntegrationEvent
(
    string Plant,
    Guid Guid,
    Guid ParentGuid,
    string ParentType,
    string Title,
    string Url,
    User CreatedBy,
    DateTime CreatedAtUtc
) : ILinkCreatedV1
{
    public LinkCreatedIntegrationEvent(Link link, string plant) : this(
        plant,
        link.Guid,
        link.ParentGuid,
        link.ParentType,
        link.Title,
        link.Url,
        new User(link.CreatedBy.Guid, link.CreatedBy.GetFullName()),
        link.CreatedAtUtc)
    { }

    public Guid MessageId { get; }  = NewId.NextGuid();
}
