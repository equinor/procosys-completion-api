using System.Threading.Tasks;
using System;
using System.Threading;
using Equinor.ProCoSys.Completion.Domain.Imports;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public interface ICheckListApiService
{
    Task<ProCoSys4CheckList?> GetCheckListAsync(string plant, Guid checkListGuid, CancellationToken cancellationToken);
    Task RecalculateCheckListStatus(string plant, Guid checkListGuid, CancellationToken cancellationToken);
    Task<ChecklistsByPunchGuidInstance> GetByPunchItemGuidAsync(string plant, Guid punchItemGuid, CancellationToken cancellationToken);
    Task<TagCheckList[]> GetCheckListsByTagIdAndPlantAsync(int tagId, string plant, CancellationToken cancellationToken);
}
