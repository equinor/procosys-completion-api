using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.WebApi.Misc;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public class AccessChecker : IAccessChecker
{
    private readonly IRestrictionRolesChecker _restrictionRolesChecker;
    private readonly ICheckListCache _checkListCache;
    private readonly IPunchItemHelper _punchItemHelper;
    private readonly IProjectAccessChecker _projectAccessChecker;

    public AccessChecker(
        IRestrictionRolesChecker restrictionRolesChecker,
        ICheckListCache checkListCache,
        IPunchItemHelper punchItemHelper,
        IProjectAccessChecker projectAccessChecker)
    {
        _restrictionRolesChecker = restrictionRolesChecker;
        _checkListCache = checkListCache;
        _punchItemHelper = punchItemHelper;
        _projectAccessChecker = projectAccessChecker;

    }

    public async Task<bool> HasCurrentUserWriteAccessToCheckListAsync(Guid checkListGuid)
    {
        if (_restrictionRolesChecker.HasCurrentUserExplicitNoRestrictions())
        {
            return true;
        }

        return await HasCurrentUserExplicitAccessToContent(checkListGuid);
    }

    public async Task<bool> HasCurrentUserWriteAccessToCheckListOwningPunchItemAsync(Guid punchItemGuid)
    {
        if (_restrictionRolesChecker.HasCurrentUserExplicitNoRestrictions())
        {
            return true;
        }

        var checkListGuid = await _punchItemHelper.GetCheckListGuidForPunchItemAsync(punchItemGuid);

        if (!checkListGuid.HasValue)
        {
            throw new Exception($"CheckListGuid for PunchItem '{punchItemGuid}' not found");
        }

        return await HasCurrentUserExplicitAccessToContent(checkListGuid.Value);
    }

    public async Task<bool> HasCurrentUserReadAccessToCheckListAsync(Guid checkListGuid)
    {
        var checkList = await _checkListCache.GetCheckListAsync(checkListGuid);
        return checkList == null
            ? throw new Exception($"CheckList with guid '{checkListGuid}' not found")
            : _projectAccessChecker.HasCurrentUserAccessToProject(checkList.ProjectGuid);
    }

    private async Task<bool> HasCurrentUserExplicitAccessToContent(Guid checkListGuid)
    {
        var checkList = await _checkListCache.GetCheckListAsync(checkListGuid);
        if (checkList is null)
        {
            throw new Exception($"CheckList '{checkListGuid}' not found");
        }

        return _restrictionRolesChecker.HasCurrentUserExplicitAccessToContent(checkList.ResponsibleCode);
    }
}
