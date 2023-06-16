using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain;

public interface IRepository<TEntity> where TEntity : EntityBase, IAggregateRoot
{
    void Add(TEntity item);

    Task<bool> Exists(int id);

    Task<TEntity?> TryGetByIdAsync(int id);

    Task<List<TEntity>> GetByIdsAsync(IEnumerable<int> id);

    void Remove(TEntity entity);

    Task<List<TEntity>> GetAllAsync();
}
