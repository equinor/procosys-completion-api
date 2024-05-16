using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Email;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.TemplateTransforming;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.MailTemplateAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.Command.Email;

public class CompletionMailService : ICompletionMailService
{
    private readonly IPlantProvider _plantProvider;
    private readonly IPersonRepository _personRepository;
    private readonly IMailTemplateRepository _mailTemplateRepository;
    private readonly ITemplateTransformer _templateTransformer;
    private readonly IEmailService _emailService;
    private readonly ILogger<CompletionMailService> _logger;
    private readonly IOptionsMonitor<ApplicationOptions> _options;

    public CompletionMailService(IPlantProvider plantProvider,
        IPersonRepository personRepository,
        IMailTemplateRepository mailTemplateRepository,
        ITemplateTransformer templateTransformer,
        IEmailService emailService,
        ILogger<CompletionMailService> logger,
        IOptionsMonitor<ApplicationOptions> options)
    {
        _plantProvider = plantProvider;
        _personRepository = personRepository;
        _mailTemplateRepository = mailTemplateRepository;
        _templateTransformer = templateTransformer;
        _emailService = emailService;
        _logger = logger;
        _options = options;
    }

    public async Task SendEmailAsync(
        string templateCode,
        dynamic emailContext,
        List<string> emailAddresses, 
        CancellationToken cancellationToken)
    {
        if (!emailAddresses.Any())
        {
            return;
        }

        var mailTemplate = await _mailTemplateRepository.GetNonVoidedByCodeAsync(_plantProvider.Plant, templateCode, cancellationToken);

        var subject = _templateTransformer.Transform(mailTemplate.Subject, emailContext);
        var body = _templateTransformer.Transform(mailTemplate.Body, emailContext);

        if (_options.CurrentValue.FakeEmail)
        {
            var fakeBody = "This email is sent to you only since fake email is enabled. " +
                           "Without fake email enabled, this mail would have been sent to " +
                           string.Join(",", emailAddresses) + "<br>---<br>" + 
                           body;
            var fakeSubject = $"{subject} (fake email)";
            var currentUser = await _personRepository.GetCurrentPersonAsync(cancellationToken);
            List<string> currentUserEmail = [currentUser.Email];

            await _emailService.SendEmailsAsync(currentUserEmail, fakeSubject, fakeBody, cancellationToken);
            _logger.LogInformation("Fake email sent. Code='{Code}'. To='{To}'", templateCode, currentUser.Email);
            return;
        }

        await _emailService.SendEmailsAsync(emailAddresses, subject, body, cancellationToken);
        _logger.LogInformation("Email sent. Code='{Code}'. To='{To}'", templateCode, string.Join(",", emailAddresses));
    }
}
