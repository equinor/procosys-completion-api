using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.WebApi.MainApi;
using Equinor.ProCoSys.Completion.WebApi.Misc;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public class ContentAccessChecker : IContentAccessChecker
{
    private readonly IRestrictionRolesChecker _restrictionRolesChecker;
    private readonly ICheckListApiService _checkListApiService;
    private readonly IPlantProvider _plantProvider;

    public ContentAccessChecker(
        IRestrictionRolesChecker restrictionRolesChecker,
        ICheckListApiService checkListApiService,
        IPlantProvider plantProvider)
    {
        _restrictionRolesChecker = restrictionRolesChecker;
        _checkListApiService = checkListApiService;
        _plantProvider = plantProvider;
    }

    public async Task<bool> HasCurrentUserAccessToCheckListAsync(Guid checkListGuid)
    {
        if (_restrictionRolesChecker.HasCurrentUserExplicitNoRestrictions())
        {
            return true;
        }

        var plant = _plantProvider.Plant;
        var checkList = await _checkListApiService.GetCheckListAsync(plant, checkListGuid);
        if (checkList is null)
        {
            throw new InValidCheckListException($"CheckList '{checkListGuid}' is not a valid CheckList in '{plant}'");
        }
        return _restrictionRolesChecker.HasCurrentUserExplicitAccessToContent(checkList.ResponsibleCode);
    }
}
