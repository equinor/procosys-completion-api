using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Permission;
using Equinor.ProCoSys.Completion.WebApi.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Query.CacheQueries;
using MediatR;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Misc;

[Authorize]
[ApiController]
[Route("Cache")]
public class CacheController : ControllerBase
{
    private readonly IPermissionCache _permissionCache;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IPermissionApiService _permissionApiService;
    private readonly IMediator _mediator;

    public CacheController(
        IPermissionCache permissionCache,
        ICurrentUserProvider currentUserProvider,
        IPermissionApiService permissionApiService,
        IMediator mediator)
    {
        _permissionCache = permissionCache;
        _currentUserProvider = currentUserProvider;
        _permissionApiService = permissionApiService;
        _mediator = mediator;
    }

    [HttpPut("Clear")]
    public async Task Clear(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken)
    {
        var currentUserOid = _currentUserProvider.GetCurrentUserOid();
        await _permissionCache.ClearAllAsync(plant, currentUserOid, cancellationToken);
    }

    [HttpGet("PermissionsFromCache")]
    public async Task<IList<string>> GetPermissions(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)] [Required]
        string plant,
        CancellationToken cancellationToken)
    {
        var currentUserOid = _currentUserProvider.GetCurrentUserOid();
        var permissions = await _permissionCache.GetPermissionsForUserAsync(plant, currentUserOid, cancellationToken);
        return permissions;
    }

    [HttpGet("PermissionsFromMain")]
    public async Task<IList<string>> GetPermissionsFromMain(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)] [Required]
        string plant,
        CancellationToken cancellationToken)
    {
        var permissions = await _permissionApiService.GetPermissionsForCurrentUserAsync(plant, cancellationToken);
        return permissions;
    }

    [HttpGet("ProjectsFromCache")]
    public async Task<IList<AccessableProject>> GetProjects(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)] [Required]
        string plant,
        CancellationToken cancellationToken)
    {
        var currentUserOid = _currentUserProvider.GetCurrentUserOid();
        var projects = await _permissionCache.GetProjectsForUserAsync(plant, currentUserOid, cancellationToken);
        return projects;
    }

    [HttpGet("PlantsFromCache")]
    public async Task<IList<string>> GetPlantsFromCache(CancellationToken cancellationToken)
    {
        var currentUserOid = _currentUserProvider.GetCurrentUserOid();
        var plants = await _permissionCache.GetPlantIdsWithAccessForUserAsync(currentUserOid, cancellationToken);
        return plants;
    }

    [HttpGet("AllPlantsFromMain")]
    public async Task<IList<AccessablePlant>> GetPlantsFromMain(CancellationToken cancellationToken)
    {
        var currentUserOid = _currentUserProvider.GetCurrentUserOid();
        var plants = await _permissionApiService.GetAllPlantsForUserAsync(currentUserOid, cancellationToken);
        return plants;
    }

    [HttpGet("PrefetchCheckListByGuid/{checkListGuid}")]
    public async Task<ActionResult> PrefetchCheckListByGuid(
        [FromRoute] Guid checkListGuid,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new PrefetchCheckListQuery(checkListGuid), cancellationToken);
        return Ok();
    }
}
