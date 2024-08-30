using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public interface ICheckListApiService
{
    // Do not pass plant to the GET endpoint for checklist in Main API due to performance. The endpoint has m2m auth, hence it doesn't require plant specific permissions
    Task<ProCoSys4CheckList?> GetCheckListAsync(Guid checkListGuid, CancellationToken cancellationToken);
    // Do not pass plant to the GET endpoint for checklist in Main API due to performance. The endpoint has m2m auth, hence it doesn't require plant specific permissions
    Task<ProCoSys4CheckListSearchResult> SearchCheckListsAsync(
        Guid projectGuid,
        string? tagNoContains,
        string? responsibleCode,
        string? tagRegisterCode,
        string? tagFunctionCode,
        string? formularType,
        int? currentPage,
        int? itemsPerPage,
        CancellationToken cancellationToken);
    // Do not pass plant to the GET endpoint for many checklists in Main API due to performance. The endpoint has m2m auth, hence it doesn't require plant specific permissions
    Task<List<ProCoSys4CheckList>> GetManyCheckListsAsync(List<Guid> checkListGuids, CancellationToken cancellationToken);

    // Recalculate endpoints MUST have plant due to VPD in PCS4 Database
    Task RecalculateCheckListStatusAsync(string plant, Guid checkListGuid, CancellationToken cancellationToken);
    // Recalculate endpoints MUST have plant due to VPD in PCS4 Database
    Task RecalculateCheckListStatusForManyAsync(string plant, List<Guid> checkListGuids, CancellationToken cancellationToken);
}
