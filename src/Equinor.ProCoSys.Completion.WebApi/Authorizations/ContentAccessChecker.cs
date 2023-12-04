using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.WebApi.Misc;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public class ContentAccessChecker : IContentAccessChecker
{
    private readonly IRestrictionRolesChecker _restrictionRolesChecker;
    private readonly ICheckListCache _checkListCache;
    private readonly IPlantProvider _plantProvider;
    private readonly IPunchItemHelper _punchItemHelper;

    public ContentAccessChecker(
        IRestrictionRolesChecker restrictionRolesChecker,
        ICheckListCache checkListCache,
        IPunchItemHelper punchItemHelper,
        IPlantProvider plantProvider)
    {
        _restrictionRolesChecker = restrictionRolesChecker;
        _checkListCache = checkListCache;
        _plantProvider = plantProvider;
        _punchItemHelper = punchItemHelper;
    }

    public async Task<bool> HasCurrentUserAccessToCheckListAsync(Guid checkListGuid)
    {
        if (_restrictionRolesChecker.HasCurrentUserExplicitNoRestrictions())
        {
            return true;
        }

        return await HasCurrentUserExplicitAccessToContent(checkListGuid);
    }

    public async Task<bool> HasCurrentUserAccessToCheckListOwningPunchItemAsync(Guid punchItemGuid)
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

    private async Task<bool> HasCurrentUserExplicitAccessToContent(Guid checkListGuid)
    {
        var plant = _plantProvider.Plant;
        var checkList = await _checkListCache.GetCheckListAsync(plant, checkListGuid);
        if (checkList is null)
        {
            throw new Exception($"CheckList '{checkListGuid}' not found in '{plant}'");
        }

        return _restrictionRolesChecker.HasCurrentUserExplicitAccessToContent(checkList.ResponsibleCode);
    }
}
