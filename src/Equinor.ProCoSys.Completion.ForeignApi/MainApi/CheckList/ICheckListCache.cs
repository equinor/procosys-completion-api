using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public interface ICheckListCache
{
    Task<ProCoSys4CheckList?> GetCheckListAsync(Guid checkListGuid, CancellationToken cancellationToken);
    Task<List<ProCoSys4CheckList>> GetManyCheckListsAsync(List<Guid> checkListGuids, CancellationToken cancellationToken);
}
