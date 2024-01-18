using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.MessageContracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.EventPublishers;

public class EventPublisher(IPublishEndpoint publishEndpoint, ILogger<EventPublisher> logger) : IEventPublisher
{
    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken) where T : class, IIntegrationEvent
        => await publishEndpoint.Publish(message,
            context =>
            {
                context.SetSessionId(message.Guid.ToString());
                logger.LogInformation("Publishing: {Message}", context.Message.ToString());
            },
            cancellationToken);
}
