using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class HistoryItemDeletedEventConsumer : IConsumer<IHistoryItemDeletedV1>
{
    private readonly ILogger<HistoryItemDeletedEventConsumer> _logger;
    private readonly IHistoryItemRepository _historyItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    // todo unit tests
    public HistoryItemDeletedEventConsumer(
        ILogger<HistoryItemDeletedEventConsumer> logger,
        IHistoryItemRepository historyItemRepository, 
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _historyItemRepository = historyItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<IHistoryItemDeletedV1> context)
    {
        var historyItemDeletedV1 = context.Message;

        var historyItemEntity = CreateHistoryItemEntity(historyItemDeletedV1);
        _historyItemRepository.Add(historyItemEntity);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
        
        _logger.LogInformation("{MessageType} message consumed: {MessageId}\n For Guid {Guid} \n {DisplayName}", 
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
