using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class PunchItemChangeHistoryEventConsumer(
    ILogger<PunchItemChangeHistoryEventConsumer> logger,
    IHistoryItemRepository historyItemRepository,
    IUnitOfWork unitOfWork)
    : IConsumer<PunchItemChangeHistoryEvent>
{
    public async Task Consume(ConsumeContext<PunchItemChangeHistoryEvent> context)
    {
        var busEvent = context.Message;

        ValidateMessage(busEvent);

        if (!await historyItemRepository.ExistsAsync(busEvent.ProCoSysGuid, context.CancellationToken))
        {
            var document = CreateHistoryItemEntity(busEvent);
            historyItemRepository.Add(document);
            await unitOfWork.SaveChangesFromSyncAsync(context.CancellationToken);
            logger.LogDebug("{EventName} Message Consumed: {MessageId} \n Guid {Guid}",
                nameof(PunchItemChangeHistoryEvent), context.MessageId, busEvent.ProCoSysGuid);
        }
        else
        {
            logger.LogDebug("{EventName} Message Message Ignored because it already exists: {MessageId} \n Guid {Guid}",
                nameof(PunchItemChangeHistoryEvent), context.MessageId, busEvent.ProCoSysGuid);
        }
    }

    private static void ValidateMessage(PunchItemChangeHistoryEvent busEvent)
    {
        if (busEvent.ProCoSysGuid == Guid.Empty)
        {
            throw new Exception($"{nameof(PunchItemChangeHistoryEvent)} is missing {nameof(PunchItemChangeHistoryEvent.ProCoSysGuid)}");
        }
        
        if (busEvent.PunchItemGuid == Guid.Empty)
        {
            throw new Exception($"{nameof(PunchItemChangeHistoryEvent)} is missing {nameof(PunchItemChangeHistoryEvent.PunchItemGuid)}");
        }
    }

    private static HistoryItem CreateHistoryItemEntity(PunchItemChangeHistoryEvent busEvent)
    {
        var property = CreateProperty(busEvent);
        var busEventChangedAt = busEvent.ChangedAt;
        if (busEventChangedAt.Kind == DateTimeKind.Unspecified)
        {
            busEventChangedAt = DateTime.SpecifyKind(busEventChangedAt, DateTimeKind.Utc);
        }
        var historyItem = new HistoryItem(busEvent.PunchItemGuid,
            $"'{property.Name}' change history imported form old ProCoSys punch item",
            Guid.Empty, // In PCS4 PUNCHLISTITEM_CHANGEHISTORY we don't have mapping to history records to Persons 
            busEvent.ChangedBy,
            busEventChangedAt,
            null,
            busEvent.ProCoSysGuid
        );
        historyItem.AddProperty(property);
        return historyItem;
    }

    private static Property CreateProperty(PunchItemChangeHistoryEvent busEvent)
    {
        var valueDisplayType = ValueDisplayType.StringAsText.ToString();
        var propertyDisplayName = MapPropertyDisplayName(busEvent.FieldName);
        var property = new Property(propertyDisplayName, valueDisplayType);
        switch (busEvent.FieldName)
        {
            // Mapping of changed fields where field is a Library value
            // STATUS_ID is also a Library value for PA / PB in PCS4.
            // In PCS5 we just map PA / PB, without the long description "Punchlist Category A", treated in default case
            case "CLEAREDBYORG_ID":
            case "PRIORITY_ID":
            case "PUNCHLISTSORTING_ID":
            case "PUNCHLISTTYPE_ID":
            case "RAISEDBYORG_ID":
                if (!string.IsNullOrEmpty(busEvent.NewValue))
                {
                    property.Value = $"{busEvent.NewValue}, {busEvent.NewValueLong}";
                }
                if (!string.IsNullOrEmpty(busEvent.OldValue))
                {
                    property.OldValue = $"{busEvent.OldValue}, {busEvent.OldValueLong}";
                }
                break;
            // For changes of DESCRIPTION in PCS4, the value is in NewValueLong and OldValueLong
            // NewValue and OldValue just keep a shortened version of max 100 chars
            // In PCS5 we just map NewValueLong and OldValueLong
            case "DESCRIPTION":
                if (!string.IsNullOrEmpty(busEvent.NewValueLong))
                {
                    property.Value = busEvent.NewValueLong;
                }
                if (!string.IsNullOrEmpty(busEvent.OldValueLong))
                {
                    property.OldValue = busEvent.OldValueLong;
                }
                break;
            // Handle all other field changes:
            //      ACTIONBYPERSON_ID, CLEARED, DRAWING_ID, DUEDATE, ESTIMATE, ORIGINALWO_ID,
            //      REJECTED, STATUS_ID, SWCR_ID, VERIFIED, WO_ID
            default:
                if (!string.IsNullOrEmpty(busEvent.NewValue))
                {
                    property.Value = busEvent.NewValue;
                }
                if (!string.IsNullOrEmpty(busEvent.OldValue))
                {
                    property.OldValue = busEvent.OldValue;
                }
                break;
        }
        return property;
    }

    private static string MapPropertyDisplayName(string fieldName)
    {
        var fieldNameToDisplayNameDictionary = new Dictionary<string, string>
        {
            { "ACTIONBYPERSON_ID", "Action by person" }, // tested
            { "CLEARED", "Cleared" }, // tested
            { "CLEAREDBYORG_ID", "Clearing by org." }, // tested
            { "DESCRIPTION", "Description" }, // tested
            { "DRAWING_ID", "Document no" }, // tested
            { "DUEDATE", "Due date" }, // tested
            { "ESTIMATE", "Estimate" }, // tested
            { "ORIGINALWO_ID", "Original WO no" }, // tested
            { "PUNCHLISTSORTING_ID", "Punch sorting" }, // tested
            { "PUNCHLISTTYPE_ID", "Punch type" }, // tested
            { "PRIORITY_ID", "Punch priority" }, // tested
            { "RAISEDBYORG_ID", "Raised by org." }, // tested
            { "REJECTED", "Rejected" }, // tested
            { "STATUS_ID", "Category" },
            { "SWCR_ID", "SWCR no" }, // tested
            { "VERIFIED", "Verified" }, // tested
            { "WO_ID", "WO no" } // tested
        };
        if (fieldNameToDisplayNameDictionary.TryGetValue(fieldName, out var name))
        {
            return name;
        }

        return fieldName;
    }
}

public record PunchItemChangeHistoryEvent(
    Guid ProCoSysGuid,
    Guid PunchItemGuid,
    string FieldName,
    string? OldValue,
    string? OldValueLong,
    string? NewValue,
    string? NewValueLong,
    string ChangedBy, // Nullable in PCS4 DB, but pr June 5, 2024, no records exists with null
    DateTime ChangedAt  // Nullable in PCS4 DB, but pr June 5, 2024, no records exists with null
);
