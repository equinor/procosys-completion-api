using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain;

public interface IRepositoryWithGuid<TEntity> : IRepository<TEntity> where TEntity : EntityBase, IAggregateRoot, IHaveGuid
{
    /// <summary>
    /// Get entity by its Guid
    /// </summary>
    /// <param name="guid">Guid of entity to get</param>
    /// <returns>The entity</returns>
    /// <exception cref="EntityNotFoundException"></exception>
    Task<TEntity> GetAsync(Guid guid);
    Task<bool> ExistsAsync(Guid guid);
}
