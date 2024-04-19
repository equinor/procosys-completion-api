using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Query.DocumentQueries;
using Equinor.ProCoSys.Completion.WebApi.Middleware;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServiceResult.ApiExtensions;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Documents;


[ApiController]
[Route("Documents")]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("Search/")]
    [AuthorizeAny(Permissions.DOCUMENT_READ, Permissions.APPLICATION_TESTER)]
    public async Task<ActionResult<IEnumerable<string>>> DocumentSearch(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [Required]
        [FromQuery] string searchPhrase)
    {
        var result = await _mediator.Send(new DocumentSearchQuery(searchPhrase), cancellationToken);
        return this.FromResult(result);
    }
}
