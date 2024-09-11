using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Query.WorkOrderQueries;
using Equinor.ProCoSys.Completion.WebApi.Middleware;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.WorkOrders;


[ApiController]
[Route("WorkOrders")]
public class WorkOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkOrdersController(IMediator mediator) => _mediator = mediator;

    [HttpGet("Search/")]
    [AuthorizeAny(Permissions.WO_READ, Permissions.APPLICATION_TESTER)]
    public async Task<ActionResult<IEnumerable<string>>> WorkOrderSearch(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [Required]
        [FromQuery] string searchPhrase)
    {
        var result = await _mediator.Send(new WorkOrderSearchQuery(searchPhrase), cancellationToken);
        return Ok(result);
    }
}
