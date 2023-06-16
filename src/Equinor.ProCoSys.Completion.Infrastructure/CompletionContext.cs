using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using IDomainMarker = Equinor.ProCoSys.Completion.Domain.IDomainMarker;

namespace Equinor.ProCoSys.Completion.Infrastructure;

public class CompletionContext : DbContext, IUnitOfWork, IReadOnlyContext
{
    public static string CompletionContextConnectionStringName = "CompletionContext";

    private readonly IPlantProvider _plantProvider;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly ICurrentUserProvider _currentUserProvider;

    public CompletionContext(
        DbContextOptions<CompletionContext> options,
        IPlantProvider plantProvider,
        IEventDispatcher eventDispatcher,
        ICurrentUserProvider currentUserProvider)
        : base(options)
    {
        _plantProvider = plantProvider;
        _eventDispatcher = eventDispatcher;
        _currentUserProvider = currentUserProvider;
    }
       
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (DebugOptions.DebugEntityFrameworkInDevelopment)
        {
            optionsBuilder.LogTo(Console.WriteLine);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        SetGlobalPlantFilter(modelBuilder);
    }      

    public static DateTimeKindConverter DateTimeKindConverter { get; } = new();

    public virtual DbSet<Person> Persons => Set<Person>();
    public virtual DbSet<Punch> Punches => Set<Punch>();
    public virtual DbSet<Project> Projects => Set<Project>();
    public virtual DbSet<Link> Links => Set<Link>();
    public virtual DbSet<Comment> Comments => Set<Comment>();
    public virtual DbSet<Attachment> Attachments => Set<Attachment>();

    private void SetGlobalPlantFilter(ModelBuilder modelBuilder)
    {
        // Set global query filter on entities inheriting from PlantEntityBase
        // https://gunnarpeipman.com/ef-core-global-query-filters/
        foreach (var type in TypeProvider.GetEntityTypes(typeof(IDomainMarker).GetTypeInfo().Assembly, typeof(PlantEntityBase)))
        {
            typeof(CompletionContext)
                .GetMethod(nameof(CompletionContext.SetGlobalQueryFilter))
                ?.MakeGenericMethod(type)
                .Invoke(this, new object[] { modelBuilder });
        }
    }

    public void SetGlobalQueryFilter<T>(ModelBuilder builder) where T : PlantEntityBase =>
        builder
            .Entity<T>()
            .HasQueryFilter(e => e.Plant == _plantProvider.Plant);

    public IQueryable<TEntity> QuerySet<TEntity>() where TEntity : class => Set<TEntity>().AsNoTracking();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);

        await SetAuditDataAsync();
        UpdateConcurrencyToken();

        try
        {
            var result = await base.SaveChangesAsync(cancellationToken);
            await DispatchPostSaveEventsAsync(cancellationToken);
            return result;
        }
        catch (DbUpdateConcurrencyException concurrencyException)
        {
            throw new ConcurrencyException("Data store operation failed. Data may have been modified or deleted since entities were loaded.", concurrencyException);
        }
    }
            
    public async Task<IDbContextTransaction> BeginTransaction(CancellationToken cancellationToken = default) 
        => await base.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        => await base.Database.CommitTransactionAsync(cancellationToken);

    private void UpdateConcurrencyToken()
    {
        var modifiedEntries = ChangeTracker
            .Entries<EntityBase>()
            .Where(x => x.State == EntityState.Modified || x.State == EntityState.Deleted);

        foreach (var entry in modifiedEntries)
        {
            var currentRowVersion = entry.CurrentValues.GetValue<byte[]>(nameof(EntityBase.RowVersion));
            var originalRowVersion = entry.OriginalValues.GetValue<byte[]>(nameof(EntityBase.RowVersion));
            for (var i = 0; i < currentRowVersion.Length; i++)
            {
                originalRowVersion[i] = currentRowVersion[i];
            }
        }
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        var entities = ChangeTracker
            .Entries<EntityBase>()
            .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
            .Select(x => x.Entity);
        await _eventDispatcher.DispatchDomainEventsAsync(entities, cancellationToken);
    }

    private async Task DispatchPostSaveEventsAsync(CancellationToken cancellationToken = default)
    {
        var entities = ChangeTracker
            .Entries<EntityBase>()
            .Where(x => x.Entity.PostSaveDomainEvents != null && x.Entity.PostSaveDomainEvents.Any())
            .Select(x => x.Entity);
        await _eventDispatcher.DispatchPostSaveEventsAsync(entities, cancellationToken);
    }

    private async Task SetAuditDataAsync()
    {
        var addedEntries = ChangeTracker
            .Entries<ICreationAuditable>()
            .Where(x => x.State == EntityState.Added)
            .ToList();
        var modifiedEntries = ChangeTracker
            .Entries<IModificationAuditable>()
            // Also update modifiedBy / modifiedAt when deleting. This to be able to log who performed the deletion
            .Where(x => x.State == EntityState.Modified || x.State == EntityState.Deleted)
            .ToList();

        if (addedEntries.Any() || modifiedEntries.Any())
        {
            var currentUserOid = _currentUserProvider.GetCurrentUserOid();
            var currentUser = await Persons.SingleOrDefaultAsync(p => p.Guid == currentUserOid);
            if (currentUser == null)
            {
                throw new Exception(
                    $"{nameof(Person)} {currentUserOid} not found when setting SetCreated / SetModified");
            }

            foreach (var entry in addedEntries)
            {
                entry.Entity.SetCreated(currentUser);
            }

            foreach (var entry in modifiedEntries)
            {
                entry.Entity.SetModified(currentUser);
            }
        }
    }
}
