using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public class ContentAccessChecker : IContentAccessChecker
{
    private readonly IRestrictionRolesChecker _restrictionRolesChecker;
    private readonly ICheckListCache _checkListCache;
    private readonly IPlantProvider _plantProvider;

    public ContentAccessChecker(
        IRestrictionRolesChecker restrictionRolesChecker,
        ICheckListCache checkListCache,
        IPlantProvider plantProvider)
    {
        _restrictionRolesChecker = restrictionRolesChecker;
        _checkListCache = checkListCache;
        _plantProvider = plantProvider;
    }

    public async Task<bool> HasCurrentUserAccessToCheckListAsync(Guid checkListGuid)
    {
        if (_restrictionRolesChecker.HasCurrentUserExplicitNoRestrictions())
        {
            return true;
        }

        var plant = _plantProvider.Plant;
        var checkList = await _checkListCache.GetCheckListAsync(plant, checkListGuid);
        if (checkList is null)
        {
            throw new Exception($"CheckList '{checkListGuid}' not found in '{plant}'");
        }
        return _restrictionRolesChecker.HasCurrentUserExplicitAccessToContent(checkList.ResponsibleCode);
    }
}
