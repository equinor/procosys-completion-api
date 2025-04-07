using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Common.Email;
using Equinor.ProCoSys.Completion.Query.MailTemplateQueries.GetAllMailTemplates;
using Equinor.TI.CommonLibrary.Mapper.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.MailTemplates;

[AuthorizeAny(Permissions.SUPERUSER, Permissions.APPLICATION_TESTER)]
[ApiController]
[Route("MailTemplates")]
public class MailTemplatesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;
    private readonly ISchemaSource _schemaSource;

    public MailTemplatesController(IMediator mediator, IEmailService emailService, IServiceProvider serviceProvide)
    {
        _mediator = mediator;
        _emailService = emailService;
        _schemaSource = serviceProvide.GetRequiredKeyedService<ISchemaSource>("TiApiSource");
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
    public void SendTestMail(CancellationToken cancellationToken)
    {
        //await _emailService.SendEmailsAsync(["eha@equinor.com"], "subject", "Body", cancellationToken);
        var schema = _schemaSource.Get("Completion", "NotCompletion");
        Console.WriteLine(schema);

    }
}
