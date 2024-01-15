using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Completion.Query.MailTemplateQueries.GetAllMailTemplates;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServiceResult.ApiExtensions;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.MailTemplates;

[AuthorizeAny(Permissions.SUPERUSER, Permissions.APPLICATION_TESTER)]
[ApiController]
[Route("MailTemplates")]
public class MailTemplatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MailTemplatesController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get all mail templates
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>List of mail templates (or empty list)</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MailTemplateDto>>> GetMailTemplates(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllMailTemplatesQuery(), cancellationToken);
        return this.FromResult(result);
    }
}
