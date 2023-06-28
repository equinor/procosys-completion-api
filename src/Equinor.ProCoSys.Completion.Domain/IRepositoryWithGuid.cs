using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain;

public interface IRepositoryWithGuid<TEntity> : IRepository<TEntity> where TEntity : EntityBase, IAggregateRoot, IHaveGuid
{
    Task<TEntity?> GetByGuidAsync(Guid guid);
}
