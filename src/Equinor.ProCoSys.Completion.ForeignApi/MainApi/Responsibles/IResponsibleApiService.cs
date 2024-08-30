using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.Responsibles;

public interface IResponsibleApiService
{
    Task<List<ProCoSys4Responsible>> GetAllAsync(string plant, CancellationToken cancellationToken);
}
