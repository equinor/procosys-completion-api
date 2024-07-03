using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.MailTemplateAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using MassTransit;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ConcurrencyException = Equinor.ProCoSys.Common.Misc.ConcurrencyException;
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
        ICurrentUserProvider currentUserProvider,
        TokenCredential credential)
        : base(options)
    {
        _plantProvider = plantProvider;
        _eventDispatcher = eventDispatcher;
        _currentUserProvider = currentUserProvider;

        // ReSharper disable once VirtualMemberCallInConstructor
        var database = Database;
        
        // Do not set AccessToken during in-memory tests or on localhost
        if (database is { ProviderName: "Microsoft.EntityFrameworkCore.SqlServer" }
            && database.GetDbConnection() is SqlConnection connection and not {DataSource: "127.0.0.1" })
        {
            connection.AccessToken = MsiAccessTokenProvider.GetAccessTokenAsync(credential).Result;
        }
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
        modelBuilder.HasSequence<int>(PunchItem.PunchItemItemNoSequence)
            .StartsAt(PunchItem.ItemNoStartsAtt).IncrementsBy(1);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        SetGlobalPlantFilter(modelBuilder);
        
        ConfigureOutBoxPattern(modelBuilder);
    }

    public static DateTimeKindConverter DateTimeKindConverter { get; } = new();
    public static LibraryTypeConverter LibraryTypeConverter { get; } = new();

    public virtual DbSet<Label> Labels => Set<Label>();
    public virtual DbSet<LabelEntity> LabelEntities => Set<LabelEntity>();
    public virtual DbSet<Person> Persons => Set<Person>();
    public virtual DbSet<PunchItem> PunchItems => Set<PunchItem>();
    public virtual DbSet<Project> Projects => Set<Project>();
    public virtual DbSet<Link> Links => Set<Link>();
    public virtual DbSet<Comment> Comments => Set<Comment>();
    public virtual DbSet<Attachment> Attachments => Set<Attachment>();
    public virtual DbSet<LibraryItem> Library => Set<LibraryItem>();
    public virtual DbSet<Classification> Classifications => Set<Classification>();
    public virtual DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public virtual DbSet<Document> Documents => Set<Document>();
    public virtual DbSet<SWCR> SWCRs => Set<SWCR>();
    public virtual DbSet<MailTemplate> MailTemplates => Set<MailTemplate>();
    public virtual DbSet<HistoryItem> History => Set<HistoryItem>();
    public virtual DbSet<Property> Properties => Set<Property>();

    // NB! This method need to be Public, if made private it will not apply
    public void SetGlobalQueryFilter<T>(ModelBuilder builder) where T : PlantEntityBase =>
        builder
            .Entity<T>()
            .HasQueryFilter(e => e.Plant == _plantProvider.Plant || e.Plant == "N/A");

    public IQueryable<TEntity> QuerySet<TEntity>() where TEntity : class => Set<TEntity>().AsNoTracking();

    public async Task<int> SaveChangesFromSyncAsync(CancellationToken cancellationToken = default)
        => await base.SaveChangesAsync(cancellationToken);

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await SetAuditDataAsync();

        // NB! DispatchDomainEventsAsync must be called AFTER SetAuditDataAsync
        // Domain Events Handlers rely on Created / Modified info set in SetAuditDataAsync
        await DispatchDomainEventsAsync(cancellationToken);
        
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

    public async Task SetAuditDataAsync()
    {
        var addedEntries = ChangeTracker
            .Entries<ICreationAuditable>()
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            // CreatedBy WILL be null after newing a new entity, before using SetCreated()
            // We want to skip entities where CreatedBy is set, since SetAuditDataAsync can be used repeatedly 
            .Where(x => x.State == EntityState.Added && x.Entity.CreatedBy is null)
            .ToList();
        var modifiedEntries = ChangeTracker
            .Entries<IModificationAuditable>()
            // Also update modifiedBy / modifiedAt when deleting ...
            // ... This to be able to create integration events with info about who performed the deletion and when
            .Where(x => x.State == EntityState.Modified || x.State == EntityState.Deleted)
            .ToList();

        if (addedEntries.Any() || modifiedEntries.Any())
        {
            var currentUserOid = _currentUserProvider.GetCurrentUserOid();
            var currentPerson = await Persons.SingleOrDefaultAsync(p => p.Guid == currentUserOid);
            if (currentPerson is null)
            {
                throw new Exception(
                    $"{nameof(Person)} {currentUserOid} not found when setting SetCreated / SetModified");
            }

            foreach (var entry in addedEntries)
            {
                entry.Entity.SetCreated(currentPerson);
            }

            foreach (var entry in modifiedEntries)
            {
                entry.Entity.SetModified(currentPerson);
            }
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default) 
        => await base.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        => await base.Database.CommitTransactionAsync(cancellationToken);

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        => await base.Database.RollbackTransactionAsync(cancellationToken);

    private static void ConfigureOutBoxPattern(ModelBuilder modelBuilder)
        => modelBuilder.AddTransactionalOutboxEntities();

    private void SetGlobalPlantFilter(ModelBuilder modelBuilder)
    {
        // todo 104163 Discuss if we need plant or not
        // Set global query filter on entities inheriting from PlantEntityBase
        // https://gunnarpeipman.com/ef-core-global-query-filters/
        foreach (var type in TypeProvider.GetEntityTypes(typeof(IDomainMarker).GetTypeInfo().Assembly, typeof(PlantEntityBase)))
        {
            typeof(CompletionContext)
                .GetMethod(nameof(SetGlobalQueryFilter))
                ?.MakeGenericMethod(type)
                .Invoke(this, new object[] { modelBuilder });
        }
    }

    /// <summary>
    /// The UpdateConcurrencyToken method is used to manage concurrency conflicts in Entity Framework. 
    /// It's responsible for ensuring that Entity Framework correctly checks for concurrency conflicts 
    /// based on the RowVersion value set in the handler method.
    /// 
    /// When SaveChanges is called, Entity Framework compares the original RowVersion value of each 
    /// modified or deleted entity with the current value in the database. If they match, it means 
    /// no one else has modified the data since it was fetched, and the changes can be saved. 
    /// If they don't match, a DbUpdateConcurrencyException is thrown, indicating a concurrency conflict.
    /// 
    /// When we manually set the RowVersion in the handler based on the client's data, 
    /// this change doesn't reflect in the OriginalValues tracked by EF. As a result, 
    /// EFs built-in concurrency check won't detect conflicts based on the client's RowVersion.
    /// The UpdateConcurrencyToken method addresses this by updating the OriginalValues to match 
    /// the manually set RowVersion. This ensures that EFs concurrency check can correctly detect 
    /// conflicts between the client's RowVersion and the current database value.
    /// </summary>
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
            .Where(x => x.Entity.DomainEvents is not null && x.Entity.DomainEvents.Any())
            .Select(x => x.Entity);
        await _eventDispatcher.DispatchDomainEventsAsync(entities, cancellationToken);
    }

    private async Task DispatchPostSaveEventsAsync(CancellationToken cancellationToken = default)
    {
        var entities = ChangeTracker
            .Entries<EntityBase>()
            .Where(x => x.Entity.PostSaveDomainEvents is not null && x.Entity.PostSaveDomainEvents.Any())
            .Select(x => x.Entity);
        await _eventDispatcher.DispatchPostSaveEventsAsync(entities, cancellationToken);
    }
}
