using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents;

public class PunchItemUnclearedEventHandler : INotificationHandler<PunchItemUnclearedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PunchItemUnclearedEventHandler> _logger;

    public PunchItemUnclearedEventHandler(IPublishEndpoint publishEndpoint, ILogger<PunchItemUnclearedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(PunchItemUnclearedDomainEvent punchItemUnclearedEvent, CancellationToken cancellationToken)
        => await _publishEndpoint.Publish(new PunchItemUnclearedIntegrationEvent(punchItemUnclearedEvent),
            context =>
            {
                context.SetSessionId(punchItemUnclearedEvent.PunchItem.Guid.ToString());
                _logger.LogInformation("Publishing: {Message}", context.Message.ToString());
            },
            cancellationToken);
}
