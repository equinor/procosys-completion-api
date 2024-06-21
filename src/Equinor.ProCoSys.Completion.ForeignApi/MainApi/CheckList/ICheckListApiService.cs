using System.Threading.Tasks;
using System;
using System.Threading;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public interface ICheckListApiService
{
    Task<ProCoSys4CheckList?> GetCheckListAsync(string plant, Guid checkListGuid);
    Task RecalculateCheckListStatus(string plant, Guid checkListGuid, CancellationToken cancellationToken);
    Task<ChecklistsByPunchGuidInstance> GetByPunchItemGuidAsync(string plant, Guid punchItemGuid);
}
