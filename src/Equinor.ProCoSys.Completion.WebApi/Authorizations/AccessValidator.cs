using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command;
using Equinor.ProCoSys.Completion.Query.PunchQueries;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

/// <summary>
/// Validates if current user has access to perform a request of type IIsProjectCommand, 
/// IIsPunchCommand or IIsPunchQuery.
/// It validates if user has access to the project of the request 
/// </summary>
public class AccessValidator : IAccessValidator
{
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IProjectAccessChecker _projectAccessChecker;
    private readonly IPunchHelper _punchHelper;
    private readonly ILogger<AccessValidator> _logger;

    public AccessValidator(
        ICurrentUserProvider currentUserProvider,
        IProjectAccessChecker projectAccessChecker,
        IPunchHelper punchHelper,
        ILogger<AccessValidator> logger)
    {
        _currentUserProvider = currentUserProvider;
        _projectAccessChecker = projectAccessChecker;
        _punchHelper = punchHelper;
        _logger = logger;
    }

    public async Task<bool> ValidateAsync<TRequest>(TRequest request) where TRequest : IBaseRequest
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var userOid = _currentUserProvider.GetCurrentUserOid();
        if (request is IIsProjectCommand projectCommand &&
            !_projectAccessChecker.HasCurrentUserAccessToProject(projectCommand.ProjectGuid))
        {
            _logger.LogWarning("Current user {UserOid} don't have access to project {ProjectCommandProjectGuid}", userOid, projectCommand.ProjectGuid);
            return false;
        }

        if (request is IIsPunchCommand punchCommand)
        {
            if (!await HasCurrentUserAccessToProjectAsync(punchCommand.PunchGuid, userOid))
            {
                return false;
            }
        }

        if (request is IIsPunchQuery punchQuery)
        {
            if (!await HasCurrentUserAccessToProjectAsync(punchQuery.PunchGuid, userOid))
            {
                return false;
            }
        }

        return true;
    }

    private async Task<bool> HasCurrentUserAccessToProjectAsync(Guid punchGuid, Guid userOid)
    {
        var projectGuid = await _punchHelper.GetProjectGuidForPunchAsync(punchGuid);
        if (projectGuid.HasValue)
        {
            var accessToProject = _projectAccessChecker.HasCurrentUserAccessToProject(projectGuid.Value);

            if (!accessToProject)
            {
                _logger.LogWarning("Current user: {UserOid} does not have access to project: {ProjectGuid}", userOid, projectGuid);
                return false;
            }
        }

        return true;
    }
}
