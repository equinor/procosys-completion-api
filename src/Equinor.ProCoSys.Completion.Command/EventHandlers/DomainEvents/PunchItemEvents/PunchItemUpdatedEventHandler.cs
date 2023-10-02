using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents;

public class PunchItemUpdatedEventHandler : INotificationHandler<PunchItemUpdatedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PunchItemUpdatedEventHandler> _logger;

    public PunchItemUpdatedEventHandler(IPublishEndpoint publishEndpoint, ILogger<PunchItemUpdatedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(PunchItemUpdatedDomainEvent punchItemUpdatedEvent, CancellationToken cancellationToken)
        => await _publishEndpoint.Publish(new PunchItemUpdatedIntegrationEvent(punchItemUpdatedEvent),
            context =>
            {
                context.SetSessionId(punchItemUpdatedEvent.PunchItem.Guid.ToString());
                _logger.LogInformation("Publishing: {Message}", context.Message.ToString());
            },
            cancellationToken);
}
