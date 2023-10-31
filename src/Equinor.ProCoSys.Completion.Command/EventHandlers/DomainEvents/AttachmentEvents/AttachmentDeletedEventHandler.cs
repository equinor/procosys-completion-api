using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.AttachmentEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentDomainEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.AttachmentEvents;

public class AttachmentDeletedEventHandler : INotificationHandler<AttachmentDeletedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AttachmentDeletedEventHandler> _logger;

    public AttachmentDeletedEventHandler(IPublishEndpoint publishEndpoint, ILogger<AttachmentDeletedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(AttachmentDeletedDomainEvent attachmentDeletedDomainEvent, CancellationToken cancellationToken)
        => await _publishEndpoint.Publish(new AttachmentDeletedIntegrationEvent(attachmentDeletedDomainEvent),
            context =>
            {
                context.SetSessionId(attachmentDeletedDomainEvent.Attachment.Guid.ToString());
                _logger.LogInformation("Publishing: {Message}", context.Message.ToString());
            },
            cancellationToken);
}
