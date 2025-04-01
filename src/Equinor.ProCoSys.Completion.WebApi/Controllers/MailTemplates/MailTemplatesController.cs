using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Common.Email;
using Equinor.ProCoSys.Completion.Query.MailTemplateQueries.GetAllMailTemplates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.MailTemplates;

[AuthorizeAny(Permissions.SUPERUSER, Permissions.APPLICATION_TESTER)]
[ApiController]
[Route("MailTemplates")]
public class MailTemplatesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;

    public MailTemplatesController(IMediator mediator, IEmailService emailService)
    {
        _mediator = mediator;
        _emailService = emailService;
    }

    /// <summary>
    /// Get all mail templates
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>List of mail templates (or empty list)</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MailTemplateDto>>> GetMailTemplates(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllMailTemplatesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("TestSendMail-Delete-After-Test")]
    public async Task<ActionResult> SendTestMail(CancellationToken cancellationToken)
    {
        await _emailService.SendEmailsAsync(["eha@equinor.com"], "subject", "Body", cancellationToken);
        return Ok();
    }
}
