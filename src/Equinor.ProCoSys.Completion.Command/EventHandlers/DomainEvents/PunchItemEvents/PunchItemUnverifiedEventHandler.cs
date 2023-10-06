using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents;

public class PunchItemUnverifiedEventHandler : INotificationHandler<PunchItemUnverifiedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PunchItemUnverifiedEventHandler> _logger;

    public PunchItemUnverifiedEventHandler(IPublishEndpoint publishEndpoint, ILogger<PunchItemUnverifiedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(PunchItemUnverifiedDomainEvent punchItemUnverifiedEvent, CancellationToken cancellationToken)
        => await _publishEndpoint.Publish(new PunchItemUpdatedIntegrationEvent(punchItemUnverifiedEvent),
            context =>
            {
                context.SetSessionId(punchItemUnverifiedEvent.PunchItem.Guid.ToString());
                _logger.LogInformation("Publishing: {Message}", context.Message.ToString());
            },
            cancellationToken);
}
