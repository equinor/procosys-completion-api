using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Validators;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

// This class is temporary while Pcs4 is master of CheckLists.
// The class will probably die when Pcs5 become master of CheckLists 
public class ProCoSys4CheckListValidator(ICheckListCache checkListCache) : ICheckListValidator
{
    public async Task<bool> TagOwningCheckListIsVoidedAsync(Guid checkListGuid,
        CancellationToken cancellationToken)
    {
        var proCoSys4CheckList = await checkListCache.GetCheckListAsync(checkListGuid, cancellationToken);
        return proCoSys4CheckList is not null && proCoSys4CheckList.IsVoided;
    }
}
