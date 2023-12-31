﻿using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents;

public class PunchItemClearedEventHandler : INotificationHandler<PunchItemClearedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PunchItemClearedEventHandler> _logger;

    public PunchItemClearedEventHandler(IPublishEndpoint publishEndpoint, ILogger<PunchItemClearedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(PunchItemClearedDomainEvent punchItemClearedEvent, CancellationToken cancellationToken)
        => await _publishEndpoint.Publish(new PunchItemUpdatedIntegrationEvent(punchItemClearedEvent),
            context =>
            {
                context.SetSessionId(punchItemClearedEvent.PunchItem.Guid.ToString());
                _logger.LogInformation("Publishing: {Message}", context.Message.ToString());
            },
            cancellationToken);
}
