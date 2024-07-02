using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public interface ICheckListCache
{
    Task<ProCoSys4CheckList?> GetCheckListAsync(Guid checkListGuid, CancellationToken cancellationToken = default);
}
