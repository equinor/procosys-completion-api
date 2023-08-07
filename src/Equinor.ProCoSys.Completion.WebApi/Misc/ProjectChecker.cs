using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Auth.Caches;
using MediatR;
using Equinor.ProCoSys.Completion.Command;
using Equinor.ProCoSys.Completion.Query;

namespace Equinor.ProCoSys.Completion.WebApi.Misc;

public class ProjectChecker : IProjectChecker
{
    private readonly IPlantProvider _plantProvider;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IPermissionCache _permissionCache;

    public ProjectChecker(IPlantProvider plantProvider,
        ICurrentUserProvider currentUserProvider,
        IPermissionCache permissionCache)
    {
        _plantProvider = plantProvider;
        _currentUserProvider = currentUserProvider;
        _permissionCache = permissionCache;
    }

    public async Task EnsureValidProjectAsync<TRequest>(TRequest request) where TRequest : IBaseRequest
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var plant = _plantProvider.Plant;
        var userOid = _currentUserProvider.GetCurrentUserOid();

        if (request is IIsProjectCommand projectCommand)
        {
            if (!await _permissionCache.IsAValidProjectForUserAsync(plant, userOid, projectCommand.ProjectGuid))
            {
                throw new InValidProjectException($"Project '{projectCommand.ProjectGuid}' is not a valid project in '{plant}'");
            }
        }
        else if (request is IIsProjectQuery projectQuery)
        {
            if (!await _permissionCache.IsAValidProjectForUserAsync(plant, userOid, projectQuery.ProjectGuid))
            {
                throw new InValidProjectException($"Project '{projectQuery.ProjectGuid}' is not a valid project in '{plant}'");
            }
        }
    }
}
