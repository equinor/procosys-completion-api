using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.LinkEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkDomainEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.LinkEvents;

public class LinkUpdatedEventHandler : INotificationHandler<LinkUpdatedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<LinkUpdatedEventHandler> _logger;

    public LinkUpdatedEventHandler(IPublishEndpoint publishEndpoint, ILogger<LinkUpdatedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(LinkUpdatedDomainEvent linkUpdatedDomainEvent, CancellationToken cancellationToken)
        => await _publishEndpoint.Publish(new LinkUpdatedIntegrationEvent(linkUpdatedDomainEvent),
            context =>
            {
                context.SetSessionId(linkUpdatedDomainEvent.Link.Guid.ToString());
                _logger.LogInformation("Publishing: {Message}", context.Message.ToString());
            },
            cancellationToken);
}
