using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public abstract class EntityRepository<TEntity> : Domain.IRepository<TEntity> where TEntity : EntityBase, IAggregateRoot
{
    protected readonly CompletionContext Context;
    protected readonly DbSet<TEntity> Set;
    protected readonly IQueryable<TEntity> DefaultQuery;

    protected EntityRepository(CompletionContext context, DbSet<TEntity> set)
        : this(context, set, set)
    {
    }

    protected EntityRepository(CompletionContext context, DbSet<TEntity> set, IQueryable<TEntity> defaultQuery)
    {
        Context = context;
        Set = set;
        DefaultQuery = defaultQuery;
    }

    public virtual void Add(TEntity entity) =>
        Set.Add(entity);

    public Task<bool> Exists(int id) =>
        DefaultQuery.AnyAsync(x => x.Id == id);

    public virtual Task<List<TEntity>> GetAllAsync() =>
        DefaultQuery.ToListAsync();

    public virtual Task<TEntity?> TryGetByIdAsync(int id) =>
        DefaultQuery.SingleOrDefaultAsync(x => x.Id == id);

    public Task<List<TEntity>> GetByIdsAsync(IEnumerable<int> ids) =>
        DefaultQuery.Where(x => ids.Contains(x.Id)).ToListAsync();

    public virtual void Remove(TEntity entity)
    {
        if (entity is IVoidable voidable)
        {
            if (!voidable.IsVoided)
            {
                throw new Exception($"{nameof(entity)} must be voided before delete");
            }
        }
        Set.Remove(entity);
    }
}
