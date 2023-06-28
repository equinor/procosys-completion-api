using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
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

    public virtual Task<TEntity?> GetByGuidAsync(Guid guid) =>
        DefaultQuery.SingleOrDefaultAsync(x => x.Guid == guid);
}
