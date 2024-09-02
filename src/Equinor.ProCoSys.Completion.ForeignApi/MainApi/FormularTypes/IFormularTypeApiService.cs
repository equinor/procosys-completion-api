using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.FormularTypes;

public interface IFormularTypeApiService
{
    Task<List<ProCoSys4FormularType>> GetAllAsync(string plant, CancellationToken cancellationToken);
}
