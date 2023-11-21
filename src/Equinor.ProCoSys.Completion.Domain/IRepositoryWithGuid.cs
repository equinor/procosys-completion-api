using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain;

public interface IRepositoryWithGuid<TEntity> : IRepository<TEntity> where TEntity : EntityBase, IAggregateRoot, IHaveGuid
{
    /// <summary>
    /// Get entity by its Guid
    /// </summary>
    /// <param name="guid">Guid of entity to get</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The entity</returns>
    /// <exception cref="EntityNotFoundException"></exception>
    Task<TEntity> GetAsync(Guid guid, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid guid, CancellationToken cancellationToken);
}
