using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Query.CheckListQueries.GetDuplicateInfo;
using Equinor.ProCoSys.Completion.Query.CheckListQueries.GetPunchItems;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using Equinor.ProCoSys.Completion.WebApi.Middleware;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.CheckLists;

[ApiController]
[Route("CheckLists")]
public class CheckListsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get all PunchItems by a checklist Guid
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid of checklist</param>
    /// <returns>List of PunchItems (or empty list)</returns>
    /// <response code="404">Checklist not found</response>
    [AuthorizeAny(Permissions.MCCR_READ, Permissions.CPCL_READ, Permissions.DCCL_READ, Permissions.APPLICATION_TESTER)]
    [AuthorizeAny(Permissions.PUNCHITEM_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet("{guid}/PunchItems")]
    public async Task<ActionResult<IEnumerable<PunchItemDetailsDto>>> GetPunchItems(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [Required]
        [FromRoute] Guid guid)
    {
        var result = await mediator.Send(new GetPunchItemsQuery(guid), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get Duplicate info by a checklist Guid
    /// </summary>
    ///  /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Checklist Guid</param>
    /// <returns>Found duplicate info</returns>
    /// <response code="404">CheckList not found</response>
    [AuthorizeAny(Permissions.MCCR_READ, Permissions.CPCL_READ, Permissions.DCCL_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet("{guid}/DuplicateInfo")]
    public async Task<ActionResult<DuplicateInfoDto>> GetDuplicateInfo(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid)
    {
        var result = await mediator.Send(new GetDuplicateInfoQuery(guid), cancellationToken);
        return result;
    }
}
