using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Microsoft.Data.SqlClient;
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

    public Task<bool> Exists(int id, CancellationToken cancellationToken) =>
        DefaultQuery.AnyAsync(x => x.Id == id, cancellationToken);

    public virtual Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken) =>
        DefaultQuery.ToListAsync(cancellationToken);

    public virtual Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        DefaultQuery.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<List<TEntity>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken) =>
        DefaultQuery.Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);

    public virtual void Remove(TEntity entity, string getParam)
    {
        var query = "SELECT Title, Body, Excerpt FROM Post WHERE Slug = " + getParam + " ORDER BY Published DESC";
        var command = new SqlCommand(query, Context.Database.GetDbConnection() as SqlConnection);
        var reader =  command.ExecuteReader();
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
