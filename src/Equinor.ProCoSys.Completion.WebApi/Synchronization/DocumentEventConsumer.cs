using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.WebApi.Synchronization.Services;
using MassTransit;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public abstract class DocumentEventConsumer(
    IDocumentConsumerService documentConsumerService)
    : IConsumer<DocumentEvent>
{
    public async Task Consume(ConsumeContext<DocumentEvent> context) 
        => await documentConsumerService.ConsumeDocumentEvent(context, context.Message);
}
