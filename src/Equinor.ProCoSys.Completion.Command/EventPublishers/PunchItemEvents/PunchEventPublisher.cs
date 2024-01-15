using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.MessageContracts.PunchItem;
using MassTransit;

namespace Equinor.ProCoSys.Completion.Command.EventPublishers.PunchItemEvents;

public class PunchEventPublisher(IPublishEndpoint publishEndpoint) : EventPublisherBase(publishEndpoint), IPunchEventPublisher
{
    public async Task<IPunchItemCreatedV1> PublishCreatedEventAsync(PunchItem punchItem, CancellationToken cancellationToken)
    {
        var integrationEvent = new PunchItemCreatedIntegrationEvent(punchItem);

        await PublishAsync(punchItem.Guid, integrationEvent, cancellationToken);

        return integrationEvent;
    }

    public async Task<IPunchItemUpdatedV1> PublishUpdatedEventAsync(PunchItem punchItem, CancellationToken cancellationToken)
    {
        var integrationEvent = new PunchItemUpdatedIntegrationEvent(punchItem);

        await PublishAsync(punchItem.Guid, integrationEvent, cancellationToken);

        return integrationEvent;
    }

    public async Task<IPunchItemDeletedV1> PublishDeletedEventAsync(PunchItem punchItem, CancellationToken cancellationToken)
    {
        var integrationEvent = new PunchItemDeletedIntegrationEvent(punchItem);

        await PublishAsync(punchItem.Guid, integrationEvent, cancellationToken);

        return integrationEvent;
    }
}
