using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchEvents;

public class PunchCreatedEventHandler : INotificationHandler<PunchCreatedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PunchCreatedEventHandler> _logger;

    public PunchCreatedEventHandler(IPublishEndpoint publishEndpoint, ILogger<PunchCreatedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(PunchCreatedEvent punchCreatedEvent, CancellationToken cancellationToken)
    {
        var sessionId = punchCreatedEvent.Punch.Guid.ToString();
        await _publishEndpoint.Publish(new PunchCreatedMessage(punchCreatedEvent),
            context => HandleContext(context, sessionId),
            cancellationToken);
    }

    private void HandleContext(PublishContext<PunchCreatedMessage> context, string sessionId)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.SetSessionId(sessionId);
        _logger.LogInformation("Published: {Message}", context.Message.ToString());
    }
}
