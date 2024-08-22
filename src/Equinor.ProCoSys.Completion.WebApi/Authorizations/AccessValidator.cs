using System;
using System.Linq;
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
    public bool HasAccess<TRequest>(TRequest request) where TRequest : IBaseRequest
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var userOid = currentUserProvider.GetCurrentUserOid();
        if (request is INeedProjectAccess projectRequest)
        {
            var projectGuidForAccessCheck = projectRequest.GetProjectGuidForAccessCheck();
            if (!projectAccessChecker.HasCurrentUserAccessToProject(projectGuidForAccessCheck))
            {
                logger.LogWarning("Current user {UserOid} doesn't have access to project {ProjectGuid}",
                    userOid, projectGuidForAccessCheck);
                return false;
            }
        }

        if (request is ICanHaveRestrictionsViaCheckList checkListRequest)
        {
            if (!accessChecker.HasCurrentUserWriteAccessToCheckList(checkListRequest.CheckListDetailsDto))
            {
                logger.LogWarning("Current user {UserOid} doesn't have write access to checkList {CheckListGuid} or other data pertaining to this checklist",
                    userOid, checkListRequest.CheckListDetailsDto.CheckListGuid);
                return false;
            }
        }

        if (request is ICanHaveRestrictionsViaManyCheckLists checkListsRequest)
        {
            if (!accessChecker.HasCurrentUserWriteAccessToAllCheckLists(checkListsRequest.CheckListDetailsDtoList))
            {
                var checkListGuids = checkListsRequest.CheckListDetailsDtoList.Select(c => c.CheckListGuid);
                logger.LogWarning("Current user {UserOid} doesn't have write access to all checkList {CheckListGuids} or other data pertaining to these checklists",
                    userOid, string.Join(",", checkListGuids));
                return false;
            }
        }

        return true;
    }
}
