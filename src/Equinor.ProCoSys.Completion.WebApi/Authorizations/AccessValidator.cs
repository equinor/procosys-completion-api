using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.Query;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

/// <summary>
/// Validates if current user has access to perform a request
/// For some request types, it validates if user has access to the project of the request
/// For some request types, it validates if user has access to content due to restriction roles
/// </summary>
public class AccessValidator : IAccessValidator
{
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IProjectAccessChecker _projectAccessChecker;
    private readonly IContentAccessChecker _contentAccessChecker;
    private readonly IPunchItemHelper _punchItemHelper;
    private readonly ILogger<AccessValidator> _logger;

    public AccessValidator(
        ICurrentUserProvider currentUserProvider,
        IProjectAccessChecker projectAccessChecker,
        IContentAccessChecker contentAccessChecker,
        IPunchItemHelper punchItemHelper,
        ILogger<AccessValidator> logger)
    {
        _currentUserProvider = currentUserProvider;
        _projectAccessChecker = projectAccessChecker;
        _contentAccessChecker = contentAccessChecker;
        _punchItemHelper = punchItemHelper;
        _logger = logger;
    }

    public async Task<bool> ValidateAsync<TRequest>(TRequest request) where TRequest : IBaseRequest
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var userOid = _currentUserProvider.GetCurrentUserOid();
        if (request is IIsProjectCommand projectCommand)
        {
            if (!_projectAccessChecker.HasCurrentUserAccessToProject(projectCommand.ProjectGuid))
            {
                _logger.LogWarning("Current user {UserOid} don't have access to project {ProjectGuid}",
                    userOid, projectCommand.ProjectGuid);
                return false;
            }
        }

        if (request is CreatePunchItemCommand createPunchItemCommand)
        {
            if (!await _contentAccessChecker.HasCurrentUserAccessToCheckListAsync(createPunchItemCommand.CheckListGuid))
            {
                _logger.LogWarning("Current user {UserOid} doesn't have access to checkList {CheckListGuid}",
                    userOid, createPunchItemCommand.CheckListGuid);
                return false;
            }
        }

        if (request is IIsProjectQuery projectQuery)
        {
            if (!_projectAccessChecker.HasCurrentUserAccessToProject(projectQuery.ProjectGuid))
            {
                _logger.LogWarning("Current user {UserOid} don't have access to project {ProjectGuid}",
                    userOid, projectQuery.ProjectGuid);
                return false;
            }
        }

        if (request is IIsPunchItemCommand punchItemCommand)
        {
            if (!await HasCurrentUserAccessToProjectAsync(punchItemCommand.PunchItemGuid, userOid))
            {
                return false;
            }
        }

        if (request is IIsPunchItemQuery punchItemQuery)
        {
            if (!await HasCurrentUserAccessToProjectAsync(punchItemQuery.PunchItemGuid, userOid))
            {
                return false;
            }
        }

        return true;
    }

    private async Task<bool> HasCurrentUserAccessToProjectAsync(Guid punchItemGuid, Guid userOid)
    {
        var projectGuid = await _punchItemHelper.GetProjectGuidForPunchItemAsync(punchItemGuid);
        if (projectGuid.HasValue)
        {
            var accessToProject = _projectAccessChecker.HasCurrentUserAccessToProject(projectGuid.Value);

            if (!accessToProject)
            {
                _logger.LogWarning("Current user: {UserOid} does not have access to project {ProjectGuid}", userOid, projectGuid);
                return false;
            }
        }

        return true;
    }
}
