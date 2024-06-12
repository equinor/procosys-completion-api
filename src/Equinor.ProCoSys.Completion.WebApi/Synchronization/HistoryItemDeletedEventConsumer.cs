using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class HistoryItemDeletedEventConsumer(
    ILogger<HistoryItemDeletedEventConsumer> logger,
    IHistoryItemRepository historyItemRepository,
    IUnitOfWork unitOfWork)
    : IConsumer<IHistoryItemDeletedV1>
{
    public async Task Consume(ConsumeContext<IHistoryItemDeletedV1> context)
    {
        var historyItemDeletedV1 = context.Message;

        var historyItemEntity = CreateHistoryItemEntity(historyItemDeletedV1);
        historyItemRepository.Add(historyItemEntity);
        await unitOfWork.SaveChangesFromSyncAsync(context.CancellationToken);
        
        logger.LogInformation("{MessageType} message consumed: {MessageId}\n For Guid {Guid} \n {DisplayName}", 
            nameof(IHistoryItemDeletedV1),
            context.MessageId, 
            historyItemDeletedV1.Guid, 
            historyItemDeletedV1.DisplayName);
    }

    private static HistoryItem CreateHistoryItemEntity(IHistoryItemDeletedV1 historyItemDeleted)
    {
        var historyItem = new HistoryItem(
            historyItemDeleted.Guid, 
            historyItemDeleted.DisplayName, 
            historyItemDeleted.EventBy.Oid,
            historyItemDeleted.EventBy.FullName,
            historyItemDeleted.EventAtUtc,
            historyItemDeleted.ParentGuid);
        return historyItem;
    }
}
