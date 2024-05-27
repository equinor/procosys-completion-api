using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.PcsServiceBus.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class SWCREventConsumer(
    ILogger<SWCREventConsumer> logger,
    IPlantSetter plantSetter,
    ISWCRRepository swcrRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserSetter currentUserSetter,
    IOptionsMonitor<ApplicationOptions> applicationOptions)
    : IConsumer<SWCREvent>
{
    public async Task Consume(ConsumeContext<SWCREvent> context)
    {
        var busEvent = context.Message;

        ValidateMessage(busEvent);
        plantSetter.SetPlant(busEvent.Plant);

        if (await swcrRepository.ExistsAsync(busEvent.ProCoSysGuid, context.CancellationToken))
        {
            var swcr = await swcrRepository.GetAsync(busEvent.ProCoSysGuid, context.CancellationToken);
            
            if (swcr.ProCoSys4LastUpdated == busEvent.LastUpdated)
            {
                logger.LogInformation("Swcr Message Ignored because LastUpdated is the same as in db\n" +
                                      "MessageId: {MessageId} \n ProCoSysGuid {ProCoSysGuid} \n " +
                                      "EventLastUpdated: {LastUpdated} \n" +
                                      "SyncedToCompletion: {SyncedTimeStamp} \n",
                    context.MessageId, busEvent.ProCoSysGuid, busEvent.LastUpdated, swcr.SyncTimestamp );
                return;
            }

            if (swcr.ProCoSys4LastUpdated > busEvent.LastUpdated)
            {
                logger.LogWarning("Swcr Message Ignored because a newer LastUpdated already exits in db\n" +
                                  "MessageId: {MessageId} \n ProCoSysGuid {ProCoSysGuid} \n " +
                                  "EventLastUpdated: {EventLastUpdated} \n" +
                                  "LastUpdatedFromDb: {LastUpdated}",
                    context.MessageId, busEvent.ProCoSysGuid, busEvent.LastUpdated, swcr.ProCoSys4LastUpdated);
                return;
            }
            MapFromEventToSWCR(busEvent, swcr);
            swcr.SyncTimestamp = DateTime.UtcNow;
        }
        else
        {
            var swcr = CreateSWCREntity(busEvent);
            swcr.SyncTimestamp = DateTime.UtcNow;
            swcrRepository.Add(swcr);
        }

        currentUserSetter.SetCurrentUserOid(applicationOptions.CurrentValue.ObjectId);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation($"{nameof(SWCREvent)} Message Consumed: {{MessageId}} \n Guid {{Guid}} \n No {{No}}",
            context.MessageId, busEvent.ProCoSysGuid, busEvent.SwcrNo);
    }

    private static void ValidateMessage(SWCREvent busEvent)
    {
        if (busEvent.ProCoSysGuid == Guid.Empty)
        {
            throw new Exception($"{nameof(SWCREvent)} is missing {nameof(SWCREvent.ProCoSysGuid)}");
        }

        if (string.IsNullOrEmpty(busEvent.Plant))
        {
            throw new Exception($"{nameof(SWCREvent)} is missing {nameof(SWCREvent.Plant)}");
        }
    }

    private static void MapFromEventToSWCR(ISwcrEventV1 busEvent, SWCR swcr)
    {
        swcr.IsVoided = busEvent.IsVoided;
        swcr.No = int.TryParse(busEvent.SwcrNo, out var intValue) ? intValue 
            : throw new Exception($"{nameof(SWCREvent.SwcrNo)} does not have a valid format");
        swcr.ProCoSys4LastUpdated = busEvent.LastUpdated;
       
    }

    private static SWCR CreateSWCREntity(ISwcrEventV1 busEvent)
    {
        var swcr = new SWCR(
            busEvent.Plant,
            busEvent.ProCoSysGuid,
            int.TryParse(busEvent.SwcrNo, out var intValue) ? intValue
                : throw new Exception($"{nameof(SWCREvent.SwcrNo)} does not have a valid format")
        ) { IsVoided = busEvent.IsVoided, ProCoSys4LastUpdated = busEvent.LastUpdated};
        return swcr;
    }
}

public record SWCREvent
(
    string EventType,
    string Plant,
    Guid ProCoSysGuid,
    string? ProjectName,
    string SwcrNo,
    string? Title,
    long SwcrId,
    Guid? CommPkgGuid,
    string? CommPkgNo,
    string? Description,
    string? Modification,
    string? Priority,
    string? System,
    string? ControlSystem,
    string? Contract,
    string? Supplier,
    string? Node,
    string? Status,
    DateTime CreatedAt,
    bool IsVoided,
    DateTime LastUpdated,
    DateOnly? DueDate,
    float? EstimatedManHours
) : ISwcrEventV1;
