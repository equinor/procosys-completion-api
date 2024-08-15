using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries;
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
        if (request is NeedProjectAccess projectRequest)
        {
            var projectGuidForAccessCheck = projectRequest.GetProjectGuidForAccessCheck();
            if (!projectAccessChecker.HasCurrentUserAccessToProject(projectGuidForAccessCheck))
            {
                logger.LogWarning("Current user {UserOid} don't have access to project {ProjectGuid}",
                    userOid, projectGuidForAccessCheck);
                return false;
            }
        }

        if (request is CanHaveCheckListRestrictionsViaCheckList checkListRequest)
        {
            var checkListGuidForWriteAccessCheck = checkListRequest.GetCheckListGuidForWriteAccessCheck();
            if (!await accessChecker.HasCurrentUserWriteAccessToCheckListAsync(checkListGuidForWriteAccessCheck, cancellationToken))
            {
                logger.LogWarning("Current user {UserOid} doesn't have write access to checkList {CheckListGuid}",
                    userOid, checkListGuidForWriteAccessCheck);
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
        if (checkList is not null && !projectAccessChecker.HasCurrentUserAccessToProject(checkList.ProjectGuid))
        {
            logger.LogWarning("Current user {UserOid} don't have access to project {ProjectGuid}",
                userOid, checkList.ProjectGuid);
            return false;
        }

        // if checklist not found, we don't deny access and return 403, but want to return not found (404)
        return true;
    }
}
