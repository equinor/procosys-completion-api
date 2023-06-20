using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Permission;
using Equinor.ProCoSys.Completion.WebApi.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Misc;

[Authorize]
[ApiController]
[Route("Cache")]
public class CacheController : ControllerBase
{
    private readonly IPermissionCache _permissionCache;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IPermissionApiService _permissionApiService;

    public CacheController(
        IPermissionCache permissionCache,
        ICurrentUserProvider currentUserProvider,
        IPermissionApiService permissionApiService)
    {
        _permissionCache = permissionCache;
        _currentUserProvider = currentUserProvider;
        _permissionApiService = permissionApiService;
    }

    [HttpPut("Clear")]
    public void Clear(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant)
    {
        var currentUserOid = _currentUserProvider.GetCurrentUserOid();
        _permissionCache.ClearAll(plant, currentUserOid);
    }

    [HttpGet("PermissionsFromCache")]
    public async Task<IList<string>> GetPermissions(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        string plant)
    {
        var currentUserOid = _currentUserProvider.GetCurrentUserOid();
        var permissions = await _permissionCache.GetPermissionsForUserAsync(plant, currentUserOid);
        return permissions;
    }

    [HttpGet("PermissionsFromMain")]
    public async Task<IList<string>> GetPermissionsFromMain(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        string plant)
    {
        var permissions = await _permissionApiService.GetPermissionsForCurrentUserAsync(plant);
        return permissions;
    }

    [HttpGet("ProjectsFromCache")]
    public async Task<IList<AccessableProject>> GetProjects(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        string plant)
    {
        var currentUserOid = _currentUserProvider.GetCurrentUserOid();
        var projects = await _permissionCache.GetProjectsForUserAsync(plant, currentUserOid);
        return projects;
    }

    [HttpGet("PlantsFromCache")]
    public async Task<IList<string>> GetPlantsFromCache()
    {
        var currentUserOid = _currentUserProvider.GetCurrentUserOid();
        var plants = await _permissionCache.GetPlantIdsWithAccessForUserAsync(currentUserOid);
        return plants;
    }

    [HttpGet("AllPlantsFromMain")]
    public async Task<IList<AccessablePlant>> GetPlantsFromMain()
    {
        var currentUserOid = _currentUserProvider.GetCurrentUserOid();
        var plants = await _permissionApiService.GetAllPlantsForUserAsync(currentUserOid);
        return plants;
    }
}
