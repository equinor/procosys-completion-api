using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents;

public class PunchItemCategoryUpdatedEventHandler : INotificationHandler<PunchItemCategoryUpdatedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PunchItemCategoryUpdatedEventHandler> _logger;

    public PunchItemCategoryUpdatedEventHandler(IPublishEndpoint publishEndpoint, ILogger<PunchItemCategoryUpdatedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(PunchItemCategoryUpdatedDomainEvent punchItemCategoryChangeDomainEvent, CancellationToken cancellationToken)
        => await _publishEndpoint.Publish(new PunchItemUpdatedIntegrationEvent(punchItemCategoryChangeDomainEvent),
            context =>
            {
                context.SetSessionId(punchItemCategoryChangeDomainEvent.PunchItem.Guid.ToString());
                _logger.LogInformation("Publishing: {Message}", context.Message.ToString());
            },
            cancellationToken);
}
