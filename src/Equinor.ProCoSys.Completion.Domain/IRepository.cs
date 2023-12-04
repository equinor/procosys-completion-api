using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain;

public interface IRepository<TEntity> where TEntity : EntityBase, IAggregateRoot
{
    void Add(TEntity item);

    Task<bool> Exists(int id, CancellationToken cancellationToken);

    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<List<TEntity>> GetByIdsAsync(IEnumerable<int> id, CancellationToken cancellationToken);

    void Remove(TEntity entity);

    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken);
}
