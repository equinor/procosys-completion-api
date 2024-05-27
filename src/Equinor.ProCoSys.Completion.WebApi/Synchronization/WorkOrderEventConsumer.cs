using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class WorkOrderEventConsumer(
    ILogger<WorkOrderEventConsumer> logger,
    IPlantSetter plantSetter,
    IWorkOrderRepository workOrderRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserSetter currentUserSetter,
    IOptionsMonitor<ApplicationOptions> applicationOptions)
    : IConsumer<WorkOrderEvent>
{
    public async Task Consume(ConsumeContext<WorkOrderEvent> context)
    {
        var busEvent = context.Message;

        ValidateMessage(busEvent);
        plantSetter.SetPlant(busEvent.Plant);

        if (await workOrderRepository.ExistsAsync(busEvent.ProCoSysGuid, context.CancellationToken))
        {
            var workOrder = await workOrderRepository.GetAsync(busEvent.ProCoSysGuid, context.CancellationToken);
            
            if (workOrder.ProCoSys4LastUpdated == busEvent.LastUpdated)
            {
                logger.LogInformation("WorkOrder Message Ignored because LastUpdated is the same as in db\n" +
                                      "MessageId: {MessageId} \n ProCoSysGuid {ProCoSysGuid} \n " +
                                      "EventLastUpdated: {LastUpdated} \n" +
                                      "SyncedToCompletion: {SyncedTimeStamp} \n",
                    context.MessageId, busEvent.ProCoSysGuid, busEvent.LastUpdated, workOrder.SyncedTimeStamp );
                return;
            }

            if (workOrder.ProCoSys4LastUpdated > busEvent.LastUpdated)
            {
                logger.LogWarning("WorkOrder Message Ignored because a newer LastUpdated already exits in db\n" +
                                  "MessageId: {MessageId} \n ProCoSysGuid {ProCoSysGuid} \n " +
                                  "EventLastUpdated: {EventLaastUpdated} \n" +
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

        currentUserSetter.SetCurrentUserOid(applicationOptions.CurrentValue.ObjectId);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation($"{nameof(WorkOrderEvent)} Message Consumed: {{MessageId}} \n Guid {{Guid}} \n No {{No}}",
            context.MessageId, busEvent.ProCoSysGuid, busEvent.WoNo);
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
        workOrder.SyncedTimeStamp = DateTime.UtcNow;
        // TODO Investigate mapping for isClosed/isVoided
        // workOrder.IsClosed = busEvent.IsVoided;
    }

    private static WorkOrder CreateWorkOrderEntity(WorkOrderEvent busEvent) =>
        new(
            busEvent.Plant,
            busEvent.ProCoSysGuid,
            busEvent.WoNo
        ) {
            ProCoSys4LastUpdated = busEvent.LastUpdated,
            SyncedTimeStamp = DateTime.UtcNow 
        };
}

public record WorkOrderEvent
(
    string Plant,
    Guid ProCoSysGuid,
    string WoNo,
    bool IsVoided,
    DateTime LastUpdated
);// :using fields from IWorkOrderEventV1;
