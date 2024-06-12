using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.WebApi.Synchronization.Services;
using MassTransit;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class QueryEventConsumer(IDocumentConsumerService documentConsumerService)
    : IConsumer<QueryEvent>
{
    public async Task Consume(ConsumeContext<QueryEvent> context)
    {
        var busEvent = context.Message;
        var documentEvent = new DocumentEvent(
            busEvent.Plant,
            busEvent.ProCoSysGuid,
            busEvent.QueryNo, //this is documentNo in oracle
            busEvent.IsVoided,
            busEvent.LastUpdated,
            busEvent.Behavior);
        await documentConsumerService.ConsumeDocumentEvent(context, documentEvent);
    }
}

public abstract record QueryEvent(
    Guid ProCoSysGuid, 
    string Plant, 
    DateTime LastUpdated,
    bool IsVoided,
    string QueryNo,
    string? Behavior);
