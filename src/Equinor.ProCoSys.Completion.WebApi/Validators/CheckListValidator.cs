using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

namespace Equinor.ProCoSys.Completion.WebApi.Validators;

public class CheckListValidator : ICheckListValidator
{
    private readonly ICheckListCache _checkListCache;
    private readonly IPlantProvider _plantProvider;

    public CheckListValidator(ICheckListCache checkListCache, IPlantProvider plantProvider)
    {
        _checkListCache = checkListCache;
        _plantProvider = plantProvider;
    }

    public async Task<bool> ExistsAsync(Guid checkListGuid)
    {
        var proCoSys4CheckList = await _checkListCache.GetCheckListAsync(_plantProvider.Plant, checkListGuid);
        return proCoSys4CheckList is not null;
    }

    public async Task<bool> TagOwningCheckListIsVoidedAsync(Guid checkListGuid)
    {
        var proCoSys4CheckList = await _checkListCache.GetCheckListAsync(_plantProvider.Plant, checkListGuid);
        return proCoSys4CheckList is not null && proCoSys4CheckList.IsVoided;
    }

    public async Task<bool> InProjectAsync(Guid checkListGuid, Guid projectGuid)
    {
        var proCoSys4CheckList = await _checkListCache.GetCheckListAsync(_plantProvider.Plant, checkListGuid);
        return proCoSys4CheckList is not null && proCoSys4CheckList.ProjectGuid == projectGuid;
    }
}
