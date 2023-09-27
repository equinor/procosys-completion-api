using System.Threading.Tasks;
using System;

namespace Equinor.ProCoSys.Completion.WebApi.MainApi;

public interface ICheckListApiService
{
    Task<ProCoSys4CheckList?> GetCheckListAsync(string plant, Guid checkListGuid);
}
