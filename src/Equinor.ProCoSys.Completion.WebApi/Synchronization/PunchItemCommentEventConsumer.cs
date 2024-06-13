using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

// ReSharper disable once ClassNeverInstantiated.Global
public class PunchItemCommentEventConsumer(
    ILogger<PunchItemCommentEventConsumer> logger,
    IPlantSetter plantSetter,
    ICommentRepository commentRepository,
    IPersonRepository personRepository,
    IUnitOfWork unitOfWork
    ) : IConsumer<PunchItemCommentEvent>
{
    public async Task Consume(ConsumeContext<PunchItemCommentEvent> context)
    {
        var busEvent = context.Message;
        plantSetter.SetPlant(busEvent.Plant);
        if (busEvent.Behavior == "delete")
        {
            logger.LogWarning("we dont do deletes for comments {Guid}", busEvent.ProCoSysGuid); 
        }else if (await commentRepository.ExistsAsync(busEvent.ProCoSysGuid, context.CancellationToken))
        {
            //No need to overwrite existing comment, its never changed
            logger.LogDebug("comment with {Guid} already exists", busEvent.ProCoSysGuid);
            return;
        }
        else
        {
            var comment = new Comment(nameof(PunchItem), busEvent.PunchItemGuid, busEvent.Text, busEvent.ProCoSysGuid);
            
            var createdBy = busEvent.CreatedByGuid is not null 
                ? await personRepository.GetAsync(busEvent.CreatedByGuid.Value, context.CancellationToken) 
                : null;
            comment.SetSyncProperties(busEvent.CreatedAt, createdBy);
            
        }

        await unitOfWork.SaveChangesFromSyncAsync(context.CancellationToken);
        
        logger.LogDebug("{EventName} Message Consumed: {MessageId} \n Guid {Guid} ",
            nameof(PunchItemCommentEvent), context.MessageId, busEvent.ProCoSysGuid);
    }
}

public record PunchItemCommentEvent(
    string Plant,
    Guid ProCoSysGuid,
    Guid PunchItemGuid, 
    string Text, 
    DateTime CreatedAt,
    Guid? CreatedByGuid,
    string? Behavior);
