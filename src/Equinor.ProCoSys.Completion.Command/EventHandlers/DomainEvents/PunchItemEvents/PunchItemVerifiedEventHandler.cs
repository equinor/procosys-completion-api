using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents;

public class PunchItemVerifiedEventHandler : INotificationHandler<PunchItemVerifiedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PunchItemVerifiedEventHandler> _logger;

    public PunchItemVerifiedEventHandler(IPublishEndpoint publishEndpoint, ILogger<PunchItemVerifiedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(PunchItemVerifiedDomainEvent punchItemVerifiedEvent, CancellationToken cancellationToken)
        => await _publishEndpoint.Publish(new PunchItemUpdatedIntegrationEvent(punchItemVerifiedEvent),
            context =>
            {
                context.SetSessionId(punchItemVerifiedEvent.PunchItem.Guid.ToString());
                _logger.LogInformation("Publishing: {Message}", context.Message.ToString());
            },
            cancellationToken);
}
