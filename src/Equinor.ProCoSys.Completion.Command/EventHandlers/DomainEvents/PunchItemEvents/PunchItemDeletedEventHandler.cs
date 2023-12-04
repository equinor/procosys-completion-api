using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents;

public class PunchItemDeletedEventHandler : INotificationHandler<PunchItemDeletedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PunchItemDeletedEventHandler> _logger;

    public PunchItemDeletedEventHandler(IPublishEndpoint publishEndpoint, ILogger<PunchItemDeletedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(PunchItemDeletedDomainEvent punchItemDeletedEvent, CancellationToken cancellationToken)
        => await _publishEndpoint.Publish(new PunchItemDeletedIntegrationEvent(punchItemDeletedEvent),
            context =>
            {
                context.SetSessionId(punchItemDeletedEvent.PunchItem.Guid.ToString());
                _logger.LogInformation("Publishing: {Message}", context.Message.ToString());
            },
            cancellationToken);
}
