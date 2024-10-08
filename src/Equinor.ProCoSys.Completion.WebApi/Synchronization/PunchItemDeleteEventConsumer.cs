using Equinor.ProCoSys.Completion.Domain;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class PunchItemDeleteEventConsumer(
    ILogger<PunchItemDeleteEventConsumer> logger,

    IPunchItemRepository punchItemRepository,
    IUnitOfWork unitOfWork)
    : IConsumer<PunchItemDeleteEvent>
{
    public async Task Consume(ConsumeContext<PunchItemDeleteEvent> context)
    {
        var busEvent = context.Message;

        ValidateMessage(busEvent);
       
        if (busEvent.Behavior == "delete")
        {
            if (await punchItemRepository.RemoveByGuidAsync(busEvent.ProCoSysGuid, context.CancellationToken))
            {
                await unitOfWork.SaveChangesFromSyncAsync(context.CancellationToken);
                logger.LogDebug("{EventName} Message {MessageId}: PunchItem with Guid {Guid} deleted",
                    nameof(PunchItemDeleteEvent), context.MessageId, busEvent.ProCoSysGuid);
            }
            else
            {
                logger.LogWarning("{EventName} Message {MessageId}: PunchItem with Guid {Guid} was not found and could not be deleted",
                    nameof(PunchItemDeleteEvent), context.MessageId, busEvent.ProCoSysGuid);
            }
        }

        logger.LogDebug("{EventName} Message Consumed: {MessageId} \n Guid {Guid} \n Behavior {Behavior}",
            nameof(PunchItemDeleteEvent), context.MessageId, busEvent.ProCoSysGuid, busEvent.Behavior);
    }

    private static void ValidateMessage(PunchItemDeleteEvent busEvent)
    {
        if (busEvent.ProCoSysGuid == Guid.Empty)
        {
            throw new Exception($"{nameof(PunchItemDeleteEvent)} is missing {nameof(PunchItemDeleteEvent.ProCoSysGuid)}");
        }
    }

}

public record PunchItemDeleteEvent(
    Guid ProCoSysGuid,
    string Plant,
    string? Behavior
);
