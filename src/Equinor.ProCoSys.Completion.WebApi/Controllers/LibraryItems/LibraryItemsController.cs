﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Query;
using Equinor.ProCoSys.Completion.Query.LibraryItemQueries.GetLibraryItems;
using Equinor.ProCoSys.Completion.WebApi.Middleware;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.LibraryItems;

[ApiController]
[Route("LibraryItems")]
public class LibraryItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LibraryItemsController(IMediator mediator) => _mediator = mediator;

    [AuthorizeAny(Permissions.PUNCHITEM_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LibraryItemDto>>> GetPunchLibraryItems(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [Required]
        [FromQuery] LibraryType[] libraryTypes)
    {
        var result = await _mediator.Send(new GetPunchLibraryItemsQuery(libraryTypes), cancellationToken);
        return Ok(result);
    }
}
