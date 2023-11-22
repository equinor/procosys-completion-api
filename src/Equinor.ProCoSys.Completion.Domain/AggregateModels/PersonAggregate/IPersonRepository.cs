using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;

public interface IPersonRepository : IRepositoryWithGuid<Person>
{
    Task<Person> GetCurrentPersonAsync(CancellationToken cancellationToken);
}
