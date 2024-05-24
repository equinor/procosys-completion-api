using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MassTransit;
using Microsoft.Extensions.Logging;
using HistoryProperty = Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate.Property;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class HistoryItemCreatedEventConsumer : IConsumer<HistoryItemCreated>
{
    private readonly ILogger<HistoryItemCreatedEventConsumer> _logger;
    private readonly IHistoryItemRepository _historyItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public HistoryItemCreatedEventConsumer(ILogger<HistoryItemCreatedEventConsumer> logger,
        IHistoryItemRepository historyItemRepository, 
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _historyItemRepository = historyItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<HistoryItemCreated> context)
    {
        var historyItemCreated = context.Message;

        var historyItem = CreateHistoryItemEntity(historyItemCreated);
        _historyItemRepository.Add(historyItem);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
        
        _logger.LogInformation("{MessageType} message consumed: {MessageId}\n For Guid {Guid} \n {DisplayName}", 
            nameof(HistoryItemCreated),
            context.MessageId, 
            historyItemCreated.Guid, 
            historyItemCreated.DisplayName);
    }

    private static HistoryItem CreateHistoryItemEntity(HistoryItemCreated historyItemCreated)
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
            historyItem.AddProperty(
                new HistoryProperty(
                    property.Name, 
                    null, 
                    property.Value?.ToString(), 
                    property.ValueDisplayType.ToString()));
        }
        return historyItem;
    }
}

public record Property(string Name, object? Value, ValueDisplayType ValueDisplayType) : IProperty;
public record User(Guid Oid, string FullName) : IUser;

// does fulfill IHistoryItemCreatedV1
public record HistoryItemCreated(
    Guid Guid,
    Guid? ParentGuid,
    string DisplayName,
    User EventBy,
    DateTime EventAtUtc,
    List<Property> Properties);
    
