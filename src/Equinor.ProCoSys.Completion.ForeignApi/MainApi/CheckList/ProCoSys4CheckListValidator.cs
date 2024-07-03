using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Validators;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

// This class is temporary while Pcs4 is master of CheckLists.
// The class will probably die when Pcs5 become master of CheckLists 
public class ProCoSys4CheckListValidator : ICheckListValidator
{
    private readonly ICheckListCache _checkListCache;

    public ProCoSys4CheckListValidator(ICheckListCache checkListCache) => _checkListCache = checkListCache;

    public async Task<bool> ExistsAsync(Guid checkListGuid, CancellationToken cancellationToken)
    {
        var proCoSys4CheckList = await _checkListCache.GetCheckListAsync(checkListGuid, cancellationToken);
        return proCoSys4CheckList is not null;
    }

    public async Task<bool> TagOwningCheckListIsVoidedAsync(Guid checkListGuid, CancellationToken cancellationToken)
    {
        var proCoSys4CheckList = await _checkListCache.GetCheckListAsync(checkListGuid, cancellationToken);
        return proCoSys4CheckList is not null && proCoSys4CheckList.IsVoided;
    }

    public async Task<bool> InProjectAsync(Guid checkListGuid, Guid projectGuid, CancellationToken cancellationToken)
    {
        var proCoSys4CheckList = await _checkListCache.GetCheckListAsync(checkListGuid, cancellationToken);
        return proCoSys4CheckList is not null && proCoSys4CheckList.ProjectGuid == projectGuid;
    }
}
