using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
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
public class AccessValidator(
    ICurrentUserProvider currentUserProvider,
    IProjectAccessChecker projectAccessChecker,
    IAccessChecker accessChecker,
    IPunchItemHelper punchItemHelper,
    ICheckListCache checkListCache,
    ILogger<AccessValidator> logger)
    : IAccessValidator
{
    public async Task<bool> ValidateAsync<TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : IBaseRequest
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var userOid = currentUserProvider.GetCurrentUserOid();
        if (request is IIsProjectCommand projectCommand)
        {
            if (!projectAccessChecker.HasCurrentUserAccessToProject(projectCommand.ProjectGuid))
            {
                logger.LogWarning("Current user {UserOid} don't have access to project {ProjectGuid}",
                    userOid, projectCommand.ProjectGuid);
                return false;
            }
        }

        if (request is CreatePunchItemCommand createPunchItemCommand)
        {
            if (!await accessChecker.HasCurrentUserWriteAccessToCheckListAsync(createPunchItemCommand.CheckListGuid, cancellationToken))
            {
                logger.LogWarning("Current user {UserOid} doesn't have write access to checkList {CheckListGuid}",
                    userOid, createPunchItemCommand.CheckListGuid);
                return false;
            }
        }

        if (request is IIsProjectQuery projectQuery)
        {
            if (!projectAccessChecker.HasCurrentUserAccessToProject(projectQuery.ProjectGuid))
            {
                logger.LogWarning("Current user {UserOid} don't have access to project {ProjectGuid}",
                    userOid, projectQuery.ProjectGuid);
                return false;
            }
        }

        if (request is IIsPunchItemCommand punchItemCommand)
        {
            if (!HasCurrentUserAccessToProject(punchItemCommand.PunchItem.Project.Guid, userOid))
            {
                return false;
            }

            if (!await accessChecker.HasCurrentUserWriteAccessToCheckListAsync(punchItemCommand.PunchItem.CheckListGuid, cancellationToken))
            {
                logger.LogWarning("Current user {UserOid} doesn't have write access to checkList owning punch {PunchItemGuid}",
                    userOid, punchItemCommand.PunchItemGuid);
                return false;
            }
        }

        if (request is IIsPunchItemQuery punchItemQuery)
        {
            if (!await HasCurrentUserAccessToProjectOwningPunchItemAsync(punchItemQuery.PunchItemGuid, userOid, cancellationToken))
            {
                return false;
            }
        }

        if (request is IIsCheckListQuery checkListQuery)
        {
            if (!await HasCurrentUserAccessToProjectOwningCheckListAsync(checkListQuery.CheckListGuid, userOid, cancellationToken))
            {
                return false;
            }
        }

        return true;
    }

    private async Task<bool> HasCurrentUserAccessToProjectOwningCheckListAsync(Guid checkListGuid, Guid userOid, CancellationToken cancellationToken)
    {
        var checkList = await checkListCache.GetCheckListAsync(checkListGuid, cancellationToken);
        if (checkList is not null)
        {
            if (!HasCurrentUserAccessToProject(checkList.ProjectGuid, userOid))
            {
                return false;
            }
        }

        // if checklist not found, we don't deny access and return 403, but want to return not found (404)
        return true;
    }

    private async Task<bool> HasCurrentUserAccessToProjectOwningPunchItemAsync(Guid punchItemGuid, Guid userOid, CancellationToken cancellationToken)
    {
        var projectGuid = await punchItemHelper.GetProjectGuidForPunchItemAsync(punchItemGuid, cancellationToken);
        if (projectGuid.HasValue)
        {
            if (!HasCurrentUserAccessToProject(projectGuid.Value, userOid))
            {
                return false;
            }
        }

        return true;
    }

    private bool HasCurrentUserAccessToProject(Guid projectGuid, Guid userOid)
    {
        var accessToProject = projectAccessChecker.HasCurrentUserAccessToProject(projectGuid);

        if (!accessToProject)
        {
            logger.LogWarning("Current user: {UserOid} does not have access to project {ProjectGuid}", userOid, projectGuid);
            return false;
        }

        return true;
    }
}
