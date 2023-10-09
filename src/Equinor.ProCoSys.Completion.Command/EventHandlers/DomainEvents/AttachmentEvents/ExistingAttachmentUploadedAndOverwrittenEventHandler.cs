using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.AttachmentEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentDomainEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.AttachmentEvents;

public class ExistingAttachmentUploadedAndOverwrittenEventHandler
    : INotificationHandler<ExistingAttachmentUploadedAndOverwrittenDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ExistingAttachmentUploadedAndOverwrittenEventHandler> _logger;

    public ExistingAttachmentUploadedAndOverwrittenEventHandler(
        IPublishEndpoint publishEndpoint,
        ILogger<ExistingAttachmentUploadedAndOverwrittenEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(ExistingAttachmentUploadedAndOverwrittenDomainEvent attachmentCreatedDomainEvent, CancellationToken cancellationToken)
        => await _publishEndpoint.Publish(new AttachmentUpdatedIntegrationEvent(attachmentCreatedDomainEvent),
            context =>
            {
                context.SetSessionId(attachmentCreatedDomainEvent.Attachment.Guid.ToString());
                _logger.LogInformation("Publishing: {Message}", context.Message.ToString());
            },
            cancellationToken);
}
