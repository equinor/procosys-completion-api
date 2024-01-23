using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.LinkEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkDomainEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.LinkEvents;

public class LinkDeletedEventHandler : INotificationHandler<LinkDeletedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<LinkDeletedEventHandler> _logger;

    public LinkDeletedEventHandler(IPublishEndpoint publishEndpoint, ILogger<LinkDeletedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(LinkDeletedDomainEvent linkDeletedDomainEvent, CancellationToken cancellationToken)
        => await _publishEndpoint.Publish(new LinkDeletedIntegrationEvent(linkDeletedDomainEvent),
            context =>
            {
                context.SetSessionId(linkDeletedDomainEvent.Link.Guid.ToString());
                _logger.LogInformation("Publishing: {Message}", context.Message.ToString());
            },
            cancellationToken);
}
