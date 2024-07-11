using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.Validators;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

// This class is temporary while Pcs4 is master of CheckLists.
// The class will probably die when Pcs5 become master of CheckLists 
public class ProCoSys4CheckListValidator(ICheckListCache checkListCache, IPlantProvider plantProvider) : ICheckListValidator
{
    public async Task<bool> ExistsAsync(Guid checkListGuid, CancellationToken cancellationToken)
    {
        var proCoSys4CheckList = await checkListCache.GetCheckListAsync(plantProvider.Plant, checkListGuid, cancellationToken);
        return proCoSys4CheckList is not null;
    }

    public async Task<bool> TagOwningCheckListIsVoidedAsync(Guid checkListGuid,
        CancellationToken cancellationToken)
    {
        var proCoSys4CheckList = await checkListCache.GetCheckListAsync(plantProvider.Plant, checkListGuid, cancellationToken);
        return proCoSys4CheckList is not null && proCoSys4CheckList.IsVoided;
    }

    public async Task<bool> InProjectAsync(Guid checkListGuid, Guid projectGuid, CancellationToken cancellationToken)
    {
        var proCoSys4CheckList = await checkListCache.GetCheckListAsync(plantProvider.Plant, checkListGuid, cancellationToken);
        return proCoSys4CheckList is not null && proCoSys4CheckList.ProjectGuid == projectGuid;
    }
}
