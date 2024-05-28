using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class HistoryItemCreatedEventConsumer : IConsumer<IHistoryItemCreatedV1>
{
    private readonly ILogger<HistoryItemCreatedEventConsumer> _logger;
    private readonly IHistoryItemRepository _historyItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    // todo unit tests
    public HistoryItemCreatedEventConsumer(
        ILogger<HistoryItemCreatedEventConsumer> logger,
        IHistoryItemRepository historyItemRepository, 
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _historyItemRepository = historyItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<IHistoryItemCreatedV1> context)
    {
        var historyItemCreatedV1 = context.Message;

        var historyItemEntity = CreateHistoryItemEntity(historyItemCreatedV1);
        _historyItemRepository.Add(historyItemEntity);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
        
        _logger.LogInformation("{MessageType} message consumed: {MessageId}\n For Guid {Guid} \n {DisplayName}", 
            nameof(IHistoryItemCreatedV1),
            context.MessageId, 
            historyItemCreatedV1.Guid, 
            historyItemCreatedV1.DisplayName);
    }

    private static HistoryItem CreateHistoryItemEntity(IHistoryItemCreatedV1 historyItemCreated)
    {
        var historyItem = new HistoryItem(
            historyItemCreated.Guid, 
            historyItemCreated.DisplayName, 
            historyItemCreated.EventBy.Oid,
            historyItemCreated.EventBy.FullName,
            historyItemCreated.EventAtUtc,
            historyItemCreated.ParentGuid);

        foreach (var property in historyItemCreated.Properties)
        {
            historyItem.AddPropertyForCreate(property.Name,
                property.Value,
                property.ValueDisplayType);
        }
        return historyItem;
    }
}
