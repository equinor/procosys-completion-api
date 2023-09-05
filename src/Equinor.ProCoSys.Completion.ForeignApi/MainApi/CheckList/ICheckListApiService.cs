using System.Threading.Tasks;
using System;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public interface ICheckListApiService
{
    Task<ProCoSys4CheckList?> GetCheckListAsync(string plant, Guid checkListGuid);
}
