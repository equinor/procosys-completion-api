using System.Threading.Tasks;
using System;
using System.Threading;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public interface ICheckListApiService
{
    Task<ProCoSys4CheckList?> GetCheckListAsync(Guid checkListGuid, CancellationToken cancellationToken);
    Task RecalculateCheckListStatus(string plant, Guid checkListGuid, CancellationToken cancellationToken);
    Task<ChecklistsByPunchGuidInstance> GetByPunchItemGuidAsync(string plant, Guid punchItemGuid);
}
