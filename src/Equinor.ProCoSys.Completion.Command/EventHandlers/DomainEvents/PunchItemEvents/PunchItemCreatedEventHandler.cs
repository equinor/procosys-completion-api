using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents;

public class PunchItemCreatedEventHandler : INotificationHandler<PunchItemCreatedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PunchItemCreatedEventHandler> _logger;

    public PunchItemCreatedEventHandler(IPublishEndpoint publishEndpoint, ILogger<PunchItemCreatedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }
    
    public async Task Handle(PunchItemCreatedEvent punchItemCreatedEvent, CancellationToken cancellationToken)
    {
        var sessionId = punchItemCreatedEvent.PunchItem.Guid.ToString();
        
        await _publishEndpoint.Publish(new PunchItemCreatedIntegrationEvent(punchItemCreatedEvent),
            context =>
            {
                context.SetSessionId(sessionId);
                _logger.LogInformation("Publishing: {Message}", context.Message.ToString());
            },
            cancellationToken);
    }
}
