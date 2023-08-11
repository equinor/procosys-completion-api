using System.Threading.Tasks;
using System;

namespace Equinor.ProCoSys.Completion.WebApi.MainApi;

public interface ICheckListApiService
{
    Task<string?> GetCheckListAsync(string plant, Guid checkListGuid);
}
