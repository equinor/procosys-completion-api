using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.TemplateTransforming;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.MailTemplateAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.MailEvents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.Command.Email;

public class CompletionMailService(
    IPlantProvider plantProvider,
    IPersonRepository personRepository,
    IMailTemplateRepository mailTemplateRepository,
    ITemplateTransformer templateTransformer,
    IMessageProducer messageProducer,
    ILogger<CompletionMailService> logger,
    IOptionsMonitor<ApplicationOptions> options)
    : ICompletionMailService
{
    public async Task SendEmailEventAsync(
        string templateCode,
        dynamic emailContext,
        List<string> emailAddresses, 
        CancellationToken cancellationToken)
    {
        if (!emailAddresses.Any())
        {
            return;
        }

        var mailTemplate = await mailTemplateRepository.GetNonVoidedByCodeAsync(plantProvider.Plant, templateCode, cancellationToken);

        var subject = templateTransformer.Transform(mailTemplate.Subject, emailContext);
        var body = templateTransformer.Transform(mailTemplate.Body, emailContext);

        if (options.CurrentValue.FakeEmail)
        {
            var fakeBody = "This email is sent to you only since fake email is enabled. " +
                           "Without fake email enabled, this mail would have been sent to " +
                           string.Join(",", emailAddresses) + "<br>---<br>" + 
                           body;
            var fakeSubject = $"{subject} (fake email)";
            var currentUser = await personRepository.GetCurrentPersonAsync(cancellationToken);
            List<string> currentUserEmail = [currentUser.Email];

            await messageProducer.SendEmailEventAsync(new SendEmailEvent(currentUserEmail, fakeSubject, fakeBody), cancellationToken);
            logger.LogInformation("Event for sending fake email sent. Code='{Code}'. To='{To}'. EMailAddresses='{EMailAddresses}'", 
                templateCode, 
                currentUser.Email, 
                string.Join(",", emailAddresses));
            return;
        }

        await messageProducer.SendEmailEventAsync(new SendEmailEvent(emailAddresses, subject, body), cancellationToken);
        logger.LogInformation("Event for sending email sent. Code='{Code}'. To='{To}'", templateCode, string.Join(",", emailAddresses));
    }
}
