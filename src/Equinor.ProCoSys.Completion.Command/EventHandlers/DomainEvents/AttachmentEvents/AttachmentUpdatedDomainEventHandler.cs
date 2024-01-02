using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.AttachmentEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentDomainEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.AttachmentEvents;

public class AttachmentUpdatedDomainEventHandler
    : INotificationHandler<AttachmentUpdatedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AttachmentUpdatedDomainEventHandler> _logger;

    public AttachmentUpdatedDomainEventHandler(
        IPublishEndpoint publishEndpoint,
        ILogger<AttachmentUpdatedDomainEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(AttachmentUpdatedDomainEvent attachmentUpdatedDomainEvent, CancellationToken cancellationToken)
        => await _publishEndpoint.Publish(new AttachmentUpdatedIntegrationEvent(attachmentUpdatedDomainEvent),
            context =>
            {
                context.SetSessionId(attachmentUpdatedDomainEvent.Attachment.Guid.ToString());
                _logger.LogInformation("Publishing: {Message}", context.Message.ToString());
            },
            cancellationToken);
}
