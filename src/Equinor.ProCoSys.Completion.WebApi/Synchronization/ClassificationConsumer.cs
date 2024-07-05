using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
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
            var toDelete = await dbContext.Classifications.SingleOrDefaultAsync(c => c.Guid == classification.ProCoSysGuid);

            if(toDelete is not null)
            {
                dbContext.Classifications.Remove(toDelete); 
                await dbContext.SaveChangesFromSyncAsync(context.CancellationToken);
            }
           
            logger.LogDebug(
                "Classification delete Message Consumed: {MessageId} \n Guid {Guid} \n",
                context.MessageId, classification.ProCoSysGuid);
            return;
        }

        var libItem = await dbContext.Library
            .IgnoreQueryFilters()
            .Include(l =>l.Classifications)
            .SingleOrDefaultAsync(l => l.Guid == classification.CommPriorityGuid, context.CancellationToken)
                      ?? throw new EntityNotFoundException<LibraryItem>(classification.CommPriorityGuid);
        
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
