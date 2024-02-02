using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventPublishers;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands;

public abstract class PunchUpdateCommandBase
{
    protected async Task<PunchItemUpdatedIntegrationEvent> PublishPunchItemUpdatedIntegrationEventsAsync(
        IIntegrationEventPublisher integrationEventPublisher,
        PunchItem punchItem,
        string historyDisplayName,
        List<IChangedProperty> changedProperties,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new PunchItemUpdatedIntegrationEvent(punchItem);
        await integrationEventPublisher.PublishAsync(integrationEvent, cancellationToken);

        var historyEvent = new HistoryUpdatedIntegrationEvent(
            historyDisplayName,
            punchItem.Guid,
            new User(punchItem.ModifiedBy!.Guid, punchItem.ModifiedBy!.GetFullName()),
            punchItem.ModifiedAtUtc!.Value,
            changedProperties);
        await integrationEventPublisher.PublishAsync(historyEvent, cancellationToken);

        return integrationEvent;
    }
}
