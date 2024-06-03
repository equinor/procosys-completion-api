using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class HistoryItemUpdatedEventConsumer(
    ILogger<HistoryItemUpdatedEventConsumer> logger,
    IPropertyHelper propertyHelper,
    IHistoryItemRepository historyItemRepository,
    IUnitOfWork unitOfWork)
    : IConsumer<IHistoryItemUpdatedV1>
{
    public async Task Consume(ConsumeContext<IHistoryItemUpdatedV1> context)
    {
        var historyItemUpdatedV1 = context.Message;

        var historyItemEntity = CreateHistoryItemEntity(historyItemUpdatedV1);
        historyItemRepository.Add(historyItemEntity);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
        
        logger.LogInformation("{MessageType} message consumed: {MessageId}\n For Guid {Guid} \n {DisplayName}", 
            nameof(IHistoryItemUpdatedV1),
            context.MessageId, 
            historyItemUpdatedV1.Guid, 
            historyItemUpdatedV1.DisplayName);
    }

    private HistoryItem CreateHistoryItemEntity(IHistoryItemUpdatedV1 historyItemUpdated)
    {
        var historyItem = new HistoryItem(
            historyItemUpdated.Guid, 
            historyItemUpdated.DisplayName, 
            historyItemUpdated.EventBy.Oid,
            historyItemUpdated.EventBy.FullName,
            historyItemUpdated.EventAtUtc,
            historyItemUpdated.ParentGuid);

        foreach (var changedProperty in historyItemUpdated.ChangedProperties)
        {
            var property = new Property(changedProperty.Name, changedProperty.ValueDisplayType.ToString());
            var oldValueAsUser = propertyHelper.GetPropertyValueAsUser(changedProperty.OldValue, changedProperty.ValueDisplayType);
            if (oldValueAsUser is null)
            {
                property.OldValue = changedProperty.OldValue?.ToString();
            }
            else
            {
                property.OldValue = oldValueAsUser.FullName;
                property.OldOidValue = oldValueAsUser.Oid;
            }

            var valueAsUser = propertyHelper.GetPropertyValueAsUser(changedProperty.Value, changedProperty.ValueDisplayType);
            if (valueAsUser is null)
            {
                property.Value = changedProperty.Value?.ToString();
            }
            else
            {
                property.Value = valueAsUser.FullName;
                property.OidValue = valueAsUser.Oid;
            }


            historyItem.AddProperty(property);
        }
        return historyItem;
    }
}
