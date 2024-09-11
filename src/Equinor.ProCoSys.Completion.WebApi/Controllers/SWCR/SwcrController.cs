using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.WebApi.Middleware;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Threading;
using MediatR;
using Equinor.ProCoSys.Completion.Query.SWCRQueries;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.SWCR;

[ApiController]
[Route("SWCR")]
public class SwcrController : ControllerBase
{
    private readonly IMediator _mediator;

    public SwcrController(IMediator mediator) => _mediator = mediator;

    [HttpGet("Search/")]
    [AuthorizeAny(Permissions.SWCR_READ, Permissions.APPLICATION_TESTER)]
    public async Task<ActionResult<IEnumerable<string>>> SwcrSearch(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [Required]
        [FromQuery] string searchPhrase)
    {
        var result = await _mediator.Send(new SWCRSearchQuery(searchPhrase), cancellationToken);
        return Ok(result);
    }
}
