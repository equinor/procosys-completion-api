using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.LinkEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkDomainEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.LinkEvents;

public class LinkCreatedEventHandler : INotificationHandler<LinkCreatedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<LinkCreatedEventHandler> _logger;

    public LinkCreatedEventHandler(IPublishEndpoint publishEndpoint, ILogger<LinkCreatedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(LinkCreatedDomainEvent linkCreatedDomainEvent, CancellationToken cancellationToken)
        => await _publishEndpoint.Publish(new LinkCreatedIntegrationEvent(linkCreatedDomainEvent),
            context =>
            {
                context.SetSessionId(linkCreatedDomainEvent.Link.Guid.ToString());
                _logger.LogInformation("Publishing: {Message}", context.Message.ToString());
            },
            cancellationToken);
}
