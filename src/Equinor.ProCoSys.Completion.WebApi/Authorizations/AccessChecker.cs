using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public class AccessChecker(IRestrictionRolesChecker restrictionRolesChecker, ICheckListCache checkListCache)
    : IAccessChecker
{
    public async Task<bool> HasCurrentUserWriteAccessToCheckListAsync(Guid checkListGuid,
        CancellationToken cancellationToken)
    {
        if (restrictionRolesChecker.HasCurrentUserExplicitNoRestrictions())
        {
            return true;
        }

        return await HasCurrentUserExplicitAccessToContent(checkListGuid, cancellationToken);
    }

    private async Task<bool> HasCurrentUserExplicitAccessToContent(Guid checkListGuid, CancellationToken cancellationToken)
    {
        var checkList = await checkListCache.GetCheckListAsync(checkListGuid, cancellationToken);
        if (checkList is null)
        {
            throw new Exception($"CheckList '{checkListGuid}' not found");
        }

        return restrictionRolesChecker.HasCurrentUserExplicitAccessToContent(checkList.ResponsibleCode);
    }
}
