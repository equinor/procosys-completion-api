using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.AttachmentEvents;
using MassTransit;
using Microsoft.Extensions.Options;


namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

// ReSharper disable once ClassNeverInstantiated.Global
public class AttachmentDeletedConsumer(IAzureBlobService azureBlobService
,IOptionsSnapshot<BlobStorageOptions> blobStorageOptions) : 
    IConsumer<AttachmentDeletedByPunchItemIntegrationEvent>,
    IConsumer<AttachmentDeletedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<AttachmentDeletedByPunchItemIntegrationEvent> context) =>
        await DeleteBlobAsync(context.Message.FullBlobPath, context.CancellationToken);
    
    public async Task Consume(ConsumeContext<AttachmentDeletedIntegrationEvent> context) 
        => await DeleteBlobAsync(context.Message.FullBlobPath, context.CancellationToken);
    
    private async Task DeleteBlobAsync(string blobPath, CancellationToken cancellationToken) =>
        await azureBlobService.DeleteAsync(
            blobStorageOptions.Value.BlobContainer,
            blobPath,
            cancellationToken);
}
