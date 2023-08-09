using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Query;
using Equinor.ProCoSys.Completion.Query.LibraryItemQueries.GetLibraryItems;
using Equinor.ProCoSys.Completion.WebApi.Middleware;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServiceResult.ApiExtensions;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.LibraryItems;

[ApiController]
[Route("LibraryItems")]
public class LibraryItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LibraryItemsController(IMediator mediator) => _mediator = mediator;

    [AuthorizeAny(Permissions.LIBRARY_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LibraryItemDto>>> GetLibraryItems(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [Required]
        [FromQuery] LibraryType type)
    {
        var result = await _mediator.Send(new GetLibraryItemsQuery(type));
        return this.FromResult(result);
    }
}
