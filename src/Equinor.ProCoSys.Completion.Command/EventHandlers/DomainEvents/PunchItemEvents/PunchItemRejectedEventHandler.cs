using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents;

public class PunchItemRejectedEventHandler : INotificationHandler<PunchItemRejectedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PunchItemRejectedEventHandler> _logger;

    public PunchItemRejectedEventHandler(IPublishEndpoint publishEndpoint, ILogger<PunchItemRejectedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(PunchItemRejectedDomainEvent punchItemRejectedEvent, CancellationToken cancellationToken)
    {
        var sessionId = punchItemRejectedEvent.PunchItem.Guid.ToString();

        await _publishEndpoint.Publish(new PunchItemRejectedIntegrationEvent(punchItemRejectedEvent),
            context =>
            {
                context.SetSessionId(sessionId);
                _logger.LogInformation("Publishing: {Message}", context.Message.ToString());
            },
            cancellationToken);
    }
}
