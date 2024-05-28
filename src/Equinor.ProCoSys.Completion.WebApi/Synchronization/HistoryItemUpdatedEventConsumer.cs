using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class HistoryItemUpdatedEventConsumer : IConsumer<IHistoryItemUpdatedV1>
{
    private readonly ILogger<HistoryItemUpdatedEventConsumer> _logger;
    private readonly IHistoryItemRepository _historyItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    // unit tests
    public HistoryItemUpdatedEventConsumer(
        ILogger<HistoryItemUpdatedEventConsumer> logger,
        IHistoryItemRepository historyItemRepository, 
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _historyItemRepository = historyItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<IHistoryItemUpdatedV1> context)
    {
        var historyItemUpdatedV1 = context.Message;

        var historyItemEntity = CreateHistoryItemEntity(historyItemUpdatedV1);
        _historyItemRepository.Add(historyItemEntity);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
        
        _logger.LogInformation("{MessageType} message consumed: {MessageId}\n For Guid {Guid} \n {DisplayName}", 
            nameof(IHistoryItemUpdatedV1),
            context.MessageId, 
            historyItemUpdatedV1.Guid, 
            historyItemUpdatedV1.DisplayName);
    }

    private static HistoryItem CreateHistoryItemEntity(IHistoryItemUpdatedV1 historyItemUpdated)
    {
        var historyItem = new HistoryItem(
            historyItemUpdated.Guid, 
            historyItemUpdated.DisplayName, 
            historyItemUpdated.EventBy.Oid,
            historyItemUpdated.EventBy.FullName,
            historyItemUpdated.EventAtUtc,
            historyItemUpdated.ParentGuid);

        foreach (var property in historyItemUpdated.ChangedProperties)
        {
            historyItem.AddPropertyForUpdate(property.Name,
                property.OldValue,
                property.Value,
                property.ValueDisplayType);
        }
        return historyItem;
    }
}
