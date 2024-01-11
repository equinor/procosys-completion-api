using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;

namespace Equinor.ProCoSys.Completion.Command.EventPublishers;

public abstract class EventPublisherBase(IPublishEndpoint publishEndpoint)
{
    protected async Task PublishAsync<T>(
        Guid guid,
        T message,
        CancellationToken cancellationToken) where T : class
        => await publishEndpoint.Publish(message,
            context => context.SetSessionId(guid.ToString()),
            cancellationToken);
}
