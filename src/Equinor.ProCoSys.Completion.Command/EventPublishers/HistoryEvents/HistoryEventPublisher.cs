using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MassTransit;

namespace Equinor.ProCoSys.Completion.Command.EventPublishers.HistoryEvents;

public class HistoryEventPublisher(IPublishEndpoint publishEndpoint) : EventPublisherBase(publishEndpoint), IHistoryEventPublisher
{
    public async Task PublishCreatedEventAsync(
        string plant,
        string displayName,
        Guid guid,
        Guid? parentGuid,
        User createdBy,
        DateTime createdAt,
        List<IProperty> properties,
        CancellationToken cancellationToken)
    {
        var historyEvent = new HistoryCreatedIntegrationEvent(plant, displayName, guid, parentGuid, createdBy, createdAt, properties);
        await PublishAsync(guid, historyEvent, cancellationToken);
    }

    public async Task PublishUpdatedEventAsync(
        string plant,
        string displayName,
        Guid guid,
        User modifiedBy,
        DateTime modifiedAt,
        List<IChangedProperty> changedProperties,
        CancellationToken cancellationToken)
    {
        var historyEvent = new HistoryUpdatedIntegrationEvent(plant, displayName, guid, modifiedBy, modifiedAt, changedProperties);
        await PublishAsync(guid, historyEvent, cancellationToken);
    }

    public async Task PublishDeletedEventAsync(
        string plant,
        string displayName,
        Guid guid,
        Guid? parentGuid,
        User deletedBy,
        DateTime deletedAt,
        CancellationToken cancellationToken)
    {
        var historyEvent = new HistoryDeletedIntegrationEvent(plant, displayName, guid, parentGuid, deletedBy, deletedAt);
        await PublishAsync(guid, historyEvent, cancellationToken);
    }
}
