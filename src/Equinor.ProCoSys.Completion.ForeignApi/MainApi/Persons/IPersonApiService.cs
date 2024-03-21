using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.Persons;

public interface IPersonApiService
{
    Task<List<ProCoSys4Person>> GetAllPersonsAsync(string plant, CancellationToken cancellationToken);
}
