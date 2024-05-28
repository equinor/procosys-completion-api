using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.MessageProducers;

public class MessageProducer(ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint, ILogger<MessageProducer> logger)
    : IMessageProducer
{
    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken) where T : class, IIntegrationEvent
        => await publishEndpoint.Publish(message,
            context =>
            {
                context.SetSessionId(message.Guid.ToString());
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

        var sender = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{shortAddress}"));
        logger.LogInformation("Sending: DisplayName: {DisplayName}, ParentGuid: {ParentGuid}", message.DisplayName, message.ParentGuid);
        await sender.Send(message, cancellationToken);
    }
}
