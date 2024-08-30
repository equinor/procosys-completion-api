using System;
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

    Task RecalculateCheckListStatus(string plant, Guid checkListGuid, CancellationToken cancellationToken);
}
