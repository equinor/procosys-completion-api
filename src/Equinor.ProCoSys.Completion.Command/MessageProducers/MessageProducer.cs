using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.AttachmentEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.MailEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.MessageProducers;

public class MessageProducer(ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint, ILogger<MessageProducer> logger)
    : IMessageProducer
{
    private const string CompletionCopyAttachmentQueue = "completion-attachment-copy-event";
    private const string CompletionSendEmailQueue = "completion-send-email-event";

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken) where T : class, IIntegrationEvent
        => await publishEndpoint.Publish(message,
            context =>
            {
                context.SetSessionId(message.Guid.ToString());
                context.MessageId = message.MessageId;
                logger.LogInformation("Publishing: {Message}", context.Message.ToString());
            },
            cancellationToken);

    public async Task SendHistoryAsync<T>(T message, CancellationToken cancellationToken) where T : class, IHistoryItem
    {
        string shortAddress;
        if (message is IHistoryItemCreatedV1)
        {
            shortAddress = QueueNames.CompletionHistoryCreated;
        } else if (message is IHistoryItemUpdatedV1)
        {
            shortAddress = QueueNames.CompletionHistoryUpdated;
        } else if (message is IHistoryItemDeletedV1)
        {
            shortAddress = QueueNames.CompletionHistoryDeleted;
        }
        else
        {
            throw new InvalidOperationException($"Unknown type {nameof(message)}");
        }

        var address = new Uri($"queue:{shortAddress}");
        var sender = await sendEndpointProvider.GetSendEndpoint(address);
        logger.LogInformation("Sending: DisplayName: {DisplayName}, ParentGuid: {ParentGuid}, Address: {Address}", 
            message.DisplayName, 
            message.ParentGuid,
            address);
        await sender.Send(message, cancellationToken);
    }

    public async Task SendCopyAttachmentEventAsync(AttachmentCopyIntegrationEvent message, CancellationToken cancellationToken)
    {
        var address = new Uri($"queue:{CompletionCopyAttachmentQueue}");
        var sender = await sendEndpointProvider.GetSendEndpoint(address);
        logger.LogInformation("Sending: Event: {EventName}, Guid: {Guid}, CopyGuid: {DestGuid}, Address: {Address}",
            nameof(message),
            message.Guid,
            message.DestGuid,
            address);
        await sender.Send(message, cancellationToken);
    }

    public async Task SendEmailEventAsync(SendEmailEvent message, CancellationToken cancellationToken)
    {
        var address = new Uri($"queue:{CompletionSendEmailQueue}");
        var sender = await sendEndpointProvider.GetSendEndpoint(address);
        logger.LogInformation("Sending: Event: {EventName}, To: {To}, Subject: {Subject}",
            nameof(message),
            string.Join(',', message.To),
            message.Subject);
        await sender.Send(message, cancellationToken);
    }
}
