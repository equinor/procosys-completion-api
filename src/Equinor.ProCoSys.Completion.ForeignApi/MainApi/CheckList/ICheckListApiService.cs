using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public interface ICheckListApiService
{
    // Do not pass plant to the GET endpoint for checklist in Main API due to performance. The endpoint has m2m auth, hence it doesn't require plant specific permissions
    Task<ProCoSys4CheckList?> GetCheckListAsync(Guid checkListGuid, CancellationToken cancellationToken);
    // todo rename with Async prefix
    Task RecalculateCheckListStatus(string plant, Guid checkListGuid, CancellationToken cancellationToken);
    Task<ChecklistsByPunchGuidInstance> GetByPunchItemGuidAsync(string plant, Guid punchItemGuid, CancellationToken cancellationToken);
    // Do not pass plant to the GET endpoint for many checklists in Main API due to performance. The endpoint has m2m auth, hence it doesn't require plant specific permissions
    Task<List<ProCoSys4CheckList>> GetManyCheckListsAsync(List<Guid> checkListGuids, CancellationToken cancellationToken);
    // todo rename with Async prefix
    Task RecalculateCheckListStatusForMany(string plant, List<Guid> checkListGuids, CancellationToken cancellationToken);
}
