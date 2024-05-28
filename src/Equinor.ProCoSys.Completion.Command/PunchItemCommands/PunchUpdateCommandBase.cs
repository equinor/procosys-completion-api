using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands;

public abstract class PunchUpdateCommandBase
{
    protected async Task<PunchItemUpdatedIntegrationEvent> PublishPunchItemUpdatedIntegrationEventsAsync(
        IMessageProducer messageProducer,
        PunchItem punchItem,
        string historyDisplayName,
        List<IChangedProperty> changedProperties,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new PunchItemUpdatedIntegrationEvent(punchItem);
        await messageProducer.PublishAsync(integrationEvent, cancellationToken);

        var historyEvent = new HistoryUpdatedIntegrationEvent(
            historyDisplayName,
            punchItem.Guid,
            // a checklist is parent of the punch. Setting null for parent here, will cause that
            // punch updates will not be shown in checklist history. For now we assume that
            // creation and deletion of punch are sufficient in checklist history
            null,
            new User(punchItem.ModifiedBy!.Guid, punchItem.ModifiedBy!.GetFullName()),
            punchItem.ModifiedAtUtc!.Value,
            changedProperties);
        await messageProducer.SendHistoryAsync(historyEvent, cancellationToken);

        return integrationEvent;
    }
}
