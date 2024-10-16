using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Email;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.MailEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class SendEmailEventConsumer (ILogger<SendEmailEventConsumer> logger, IEmailService emailService) 
    : IConsumer<SendEmailEvent>
{
    public async Task Consume(ConsumeContext<SendEmailEvent> context)
    {
        var busEvent = context.Message;

        await emailService.SendEmailsAsync(busEvent.To, busEvent.Subject, busEvent.Body, context.CancellationToken);

        logger.LogInformation("{EventName} Message Consumed: {MessageId} \n To '{To}' \n Subject {Subject}",
            nameof(SendEmailEvent), context.MessageId, string.Join(",", busEvent.To), busEvent.Subject);
    }
}
