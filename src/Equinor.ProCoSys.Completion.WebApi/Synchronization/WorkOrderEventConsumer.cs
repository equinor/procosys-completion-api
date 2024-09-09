using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class WorkOrderEventConsumer(
    ILogger<WorkOrderEventConsumer> logger,
    IWorkOrderRepository workOrderRepository,
    IUnitOfWork unitOfWork)
    : IConsumer<WorkOrderEvent>
{
    public async Task Consume(ConsumeContext<WorkOrderEvent> context)
    {
        var busEvent = context.Message;
        ValidateMessage(busEvent);

        if (busEvent.Behavior == "delete")
        {
            if (!await workOrderRepository.RemoveByGuidAsync(busEvent.ProCoSysGuid, context.CancellationToken))
            {
                logger.LogWarning("WorkOrder with Guid {Guid} was not found and could not be deleted",
                    busEvent.ProCoSysGuid);
            }
        }
        else if (await workOrderRepository.ExistsAsync(busEvent.ProCoSysGuid, context.CancellationToken))
        {
            var workOrder = await workOrderRepository.GetAsync(busEvent.ProCoSysGuid, context.CancellationToken);
            
            if (workOrder.ProCoSys4LastUpdated == busEvent.LastUpdated)
            {
                logger.LogInformation("WorkOrder Message Ignored because LastUpdated is the same as in db\n" +
                                      "MessageId: {MessageId} \n ProCoSysGuid: {ProCoSysGuid} \n " +
                                      "EventLastUpdated: {LastUpdated} \n" +
                                      "SyncedToCompletion: {SyncedTimeStamp} \n",
                    context.MessageId, busEvent.ProCoSysGuid, busEvent.LastUpdated, workOrder.SyncTimestamp );
                return;
            }

            if (workOrder.ProCoSys4LastUpdated > busEvent.LastUpdated)
            {
                logger.LogWarning("WorkOrder Message Ignored because a newer LastUpdated already exits in db\n" +
                                  "MessageId: {MessageId} \n ProCoSysGuid: {ProCoSysGuid} \n " +
                                  "EventLastUpdated: {EventLastUpdated} \n" +
                                  "LastUpdatedFromDb: {LastUpdated}",
                    context.MessageId, busEvent.ProCoSysGuid, busEvent.LastUpdated, workOrder.ProCoSys4LastUpdated);
                return;
            }
            
            MapFromEventToWorkOrder(busEvent, workOrder);
        }
        else
        {
            var workOrder = CreateWorkOrderEntity(busEvent);
            workOrderRepository.Add(workOrder);
        }

        await unitOfWork.SaveChangesFromSyncAsync(context.CancellationToken);

        logger.LogDebug("{EventName} Message Consumed: {MessageId} \n Guid {Guid} \n No {No}",
            nameof(WorkOrderEvent), context.MessageId, busEvent.ProCoSysGuid, busEvent.WoNo);
    }

    private static void ValidateMessage(WorkOrderEvent busEvent)
    {
        if (busEvent.ProCoSysGuid == Guid.Empty)
        {
            throw new Exception($"{nameof(WorkOrderEvent)} is missing {nameof(WorkOrderEvent.ProCoSysGuid)}");
        }

        if (string.IsNullOrEmpty(busEvent.Plant))
        {
            throw new Exception($"{nameof(WorkOrderEvent)} is missing {nameof(WorkOrderEvent.Plant)}");
        }
    }

    private static void MapFromEventToWorkOrder(WorkOrderEvent busEvent, WorkOrder workOrder)
    {
        workOrder.No = busEvent.WoNo;
        workOrder.ProCoSys4LastUpdated = busEvent.LastUpdated;
        workOrder.SyncTimestamp = DateTime.UtcNow;
        workOrder.IsVoided = busEvent.IsVoided;
    }

    private static WorkOrder CreateWorkOrderEntity(WorkOrderEvent busEvent) =>
        new(
            busEvent.Plant,
            busEvent.ProCoSysGuid,
            busEvent.WoNo
        ) {
            ProCoSys4LastUpdated = busEvent.LastUpdated,
            SyncTimestamp = DateTime.UtcNow,
            IsVoided = busEvent.IsVoided
        };
}

public record WorkOrderEvent
(
    string Plant,
    Guid ProCoSysGuid,
    string WoNo,
    bool IsVoided,
    DateTime LastUpdated,
    string? Behavior
);// :using fields from IWorkOrderEventV1;
