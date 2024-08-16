using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command;
using Equinor.ProCoSys.Completion.Domain;
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
                logger.LogWarning("Current user {UserOid} doesn't have access to project {ProjectGuid}",
                    userOid, projectGuidForAccessCheck);
                return false;
            }
        }

        if (request is CanHaveRestrictionsViaCheckList checkListRequest)
        {
            var checkListGuidForWriteAccessCheck = checkListRequest.GetCheckListGuidForWriteAccessCheck();
            if (!await accessChecker.HasCurrentUserWriteAccessToCheckListAsync(checkListGuidForWriteAccessCheck, cancellationToken))
            {
                logger.LogWarning("Current user {UserOid} doesn't have write access to checkList {CheckListGuid} or other data pertaining to this checklist",
                    userOid, checkListGuidForWriteAccessCheck);
                return false;
            }
        }

        return true;
    }
}
