using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public abstract class EntityWithGuidRepository<TEntity>(
    CompletionContext context,
    DbSet<TEntity> set,
    IQueryable<TEntity> defaultQueryable)
    : EntityRepository<TEntity>(context, set, defaultQueryable)
    where TEntity : EntityBase, IAggregateRoot, IHaveGuid
{
    protected EntityWithGuidRepository(CompletionContext context, DbSet<TEntity> set)
        : this(context, set, set)
    {
    }
    
    public async Task<bool> RemoveByGuidAsync(Guid guid, CancellationToken cancellationToken)
    {
        var entity = await Set.SingleOrDefaultAsync(e => e.Guid == guid, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        Set.Remove(entity);
        return true;
    }

    public virtual async Task<TEntity> GetAsync(
        Guid guid,
        CancellationToken cancellationToken)
        => await DefaultQueryable
               .SingleOrDefaultAsync(x => x.Guid == guid, cancellationToken)
           ?? throw new EntityNotFoundException<TEntity>(guid);

    public virtual async Task<bool> ExistsAsync(Guid guid, CancellationToken cancellationToken)
        => await Set.AnyAsync(e => e.Guid == guid, cancellationToken);
}
