using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;

public interface IPersonRepository : IRepositoryWithGuid<Person>
{
    Task<Person> GetCurrentPersonAsync(CancellationToken cancellationToken);
    Task<Person> GetOrCreateAsync(Guid oid, CancellationToken cancellationToken);
    Task<List<Person>> GetOrCreateManyAsync(IEnumerable<Guid> oids, CancellationToken cancellationToken);
}
