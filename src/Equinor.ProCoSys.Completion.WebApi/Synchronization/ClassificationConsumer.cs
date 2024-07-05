using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.PcsServiceBus.Interfaces;
using JetBrains.Annotations;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

[UsedImplicitly]
public class ClassificationConsumer(
    ILogger<ClassificationConsumer> logger,
    CompletionContext dbContext) 
    : IConsumer<ClassificationEvent>
{
    public async Task Consume(ConsumeContext<ClassificationEvent> context)
    {
        var classification = context.Message;
        
        if (classification.Behavior == "delete")
        {
            //ExecuteDeleteAsync() does its own save, so we don't need to call SaveChangesAsync
            //We do it this way because we don't have libraryItemGuid in the delete messages
           var deleted = await dbContext.Classifications.Where(c => c.Guid == classification.ProCoSysGuid).ExecuteDeleteAsync();
            logger.LogDebug("Classification delete Message Consumed: {MessageId} \n Guid {Guid} \n Deleted {Deleted} rows",
                 context.MessageId, classification.ProCoSysGuid, deleted);
            return;
        }

        var libItem = await dbContext.Library
            .IgnoreQueryFilters()
            .Include(l =>l.Classifications)
            .SingleAsync(l => l.Guid == classification.CommPriorityGuid, context.CancellationToken);
        
        libItem.AddClassification(new Classification(classification.ProCoSysGuid, Classification.PunchPriority));
        
        await dbContext.SaveChangesFromSyncAsync(context.CancellationToken);
        
        logger.LogDebug("{EventName} Message Consumed: {MessageId} \n Guid {Guid}",
            nameof(IPunchPriorityLibRelationEventV1), context.MessageId, classification.ProCoSysGuid);
    }
}

[UsedImplicitly]
public record ClassificationEvent(
    string EventType,
    string Plant,
    Guid ProCoSysGuid,
    DateTime LastUpdated,
    Guid CommPriorityGuid,
    string? Behavior) 
    : IPunchPriorityLibRelationEventV1;
