using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.WebApi.Synchronization.Services;
using MassTransit;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

// ReSharper disable once ClassNeverInstantiated.Global
public class DocumentEventConsumer(
    IDocumentConsumerService documentConsumerService)
    : IConsumer<DocumentEvent>
{
    public async Task Consume(ConsumeContext<DocumentEvent> context) 
        => await documentConsumerService.ConsumeDocumentEvent(context, context.Message, "Document");
}
