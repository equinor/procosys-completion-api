using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public abstract class EntityWithGuidRepository<TEntity> : EntityRepository<TEntity> where TEntity : EntityBase, IAggregateRoot, IHaveGuid
{
    protected EntityWithGuidRepository(CompletionContext context, DbSet<TEntity> set)
        : this(context, set, set)
    {
    }

    protected EntityWithGuidRepository(CompletionContext context, DbSet<TEntity> set, IQueryable<TEntity> defaultQuery)
    : base(context, set, defaultQuery)
    {
    }

    public virtual async Task<TEntity> GetAsync(Guid guid, CancellationToken cancellationToken)
    {
        var entity = await DefaultQuery.SingleOrDefaultAsync(x => x.Guid == guid, cancellationToken);
        if (entity is null)
        {
            var typeName = typeof(TEntity).Name;
            throw new EntityNotFoundException($"Could not find {typeName} with Guid {guid}");
        }
        return entity;
    }

    public virtual async Task<bool> ExistsAsync(Guid guid, CancellationToken cancellationToken)
        => await Set.AnyAsync(e => e.Guid == guid, cancellationToken);
}
