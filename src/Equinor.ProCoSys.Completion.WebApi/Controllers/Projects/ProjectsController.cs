using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Query.ProjectQueries.GetPunchItems;
using Equinor.ProCoSys.Completion.Query.ProjectQueries.SearchCheckLists;
using Equinor.ProCoSys.Completion.WebApi.Middleware;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Projects;

[ApiController]
[Route("Projects")]
public class ProjectsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Search for checklists in project
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid of project where to search</param>
    /// <param name="multipleTagNo">Set false to search for checklist where TagNo contains given string.
    /// Set true to search for checklist where TagNo exists in given string. An exact match in an "In"-search is performed.
    /// Default is false</param>
    /// <param name="tagNoContains">TagNo phrase to search for. Case-insensitive. Optional</param>
    /// <param name="responsibleCode">Search for checklist where responsibleCode equals given string. Optional</param>
    /// <param name="registerAndTagFunctionCode">Search for checklist where registerCode and tagFunctionCode equals given string.
    /// The string must be given in format "registerCode/tagFunctionCode". Sample: "MANUAL_VALVE/WL" Optional, used when multipleTagNo is false</param>
    /// <param name="formularType">Search for checklist where formularType equals given string. Optional</param>
    /// <param name="currentPage">Current page to get. Default is 0 (first page)</param>
    /// <param name="itemsPerPage">Number of items pr page. Default is 30</param>
    /// <returns>Search result</returns>
    /// <response code="404">ImportMessageProject not found</response>
    [AuthorizeAny(Permissions.MCCR_READ, Permissions.CPCL_READ, Permissions.DCCL_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet("{guid}/CheckLists/Search")]
    public async Task<ActionResult<SearchResultDto>> SearchCheckLists(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [Required]
        [FromRoute] Guid guid,
        bool multipleTagNo = false,
        string? tagNoContains = null,
        string? responsibleCode = null,
        string? registerAndTagFunctionCode = null,
        string? formularType = null,
        int? currentPage = 0,
        int? itemsPerPage = 30)
    {
        var result = await mediator.Send(new SearchCheckListsQuery(guid,
            multipleTagNo,
            tagNoContains,
            responsibleCode,
            registerAndTagFunctionCode,
            formularType,
            currentPage,
            itemsPerPage), cancellationToken);
        return result;
    }

    /// <summary>
    /// Get all PunchItems in project (no filtering available yet)
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid of project where to search</param>
    /// <returns>List of PunchItems (or empty list)</returns>
    /// <response code="404">ImportMessageProject not found</response>
    [AuthorizeAny(Permissions.PUNCHITEM_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet("{guid}/PunchItems")]
    public async Task<ActionResult<IEnumerable<PunchItemDto>>> GetPunchItems(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [Required][FromRoute] Guid guid)
    {
        var result = await mediator.Send(new GetPunchItemsQuery(guid), cancellationToken);
        return Ok(result);
    }
}
