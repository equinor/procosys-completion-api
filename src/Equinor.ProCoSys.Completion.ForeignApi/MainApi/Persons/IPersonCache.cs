using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.Persons;

public interface IPersonCache
{
    Task<List<ProCoSys4Person>> GetAllPersonsAsync(string plant, CancellationToken cancellationToken);
}
