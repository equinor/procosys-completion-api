﻿using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents;

public class PunchItemCreatedEventHandler : INotificationHandler<PunchItemCreatedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PunchItemCreatedEventHandler> _logger;

    public PunchItemCreatedEventHandler(IPublishEndpoint publishEndpoint, ILogger<PunchItemCreatedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(PunchItemCreatedDomainEvent punchItemCreatedEvent, CancellationToken cancellationToken)
        => await _publishEndpoint.Publish(new PunchItemCreatedIntegrationEvent(punchItemCreatedEvent),
            context =>
            {
                context.SetSessionId(punchItemCreatedEvent.PunchItem.Guid.ToString());
                _logger.LogInformation("Publishing: {Message}", context.Message.ToString());
            },
            cancellationToken);
}
