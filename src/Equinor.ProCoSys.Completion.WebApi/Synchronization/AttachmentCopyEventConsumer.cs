using System.Threading.Tasks;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.AttachmentEvents;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class AttachmentCopyEventConsumer (
    ILogger<AttachmentCopyEventConsumer> logger,
    IAzureBlobService azureBlobService, 
    IOptionsSnapshot<BlobStorageOptions> blobStorageOptions) 
    : IConsumer<AttachmentCopyIntegrationEvent>
{
    public async Task Consume(ConsumeContext<AttachmentCopyIntegrationEvent> context)
    {
        var busEvent = context.Message;

        await azureBlobService.CopyBlobAsync(
            blobStorageOptions.Value.BlobContainer,
            busEvent.SrcBlobPath,
            busEvent.DestBlobPath,
            true,
            context.CancellationToken
        );

        logger.LogDebug("{EventName} Message Consumed: {MessageId} \n Source Guid {Source} \n Destination Guid {Target}",
            nameof(AttachmentCopyIntegrationEvent), context.MessageId, busEvent.Guid, busEvent.DestGuid);
    }
}

