using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Imports;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public interface ICheckListCache
{
    Task<ProCoSys4CheckList?> GetCheckListAsync(string plant, Guid checkListGuid, CancellationToken cancellationToken);
    Task<TagCheckList[]> GetCheckListsByTagIdAsync(int tagId, string plant, CancellationToken cancellationToken);
}
