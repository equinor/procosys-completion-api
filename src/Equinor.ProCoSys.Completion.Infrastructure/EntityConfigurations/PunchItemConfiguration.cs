using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations;

internal class PunchItemConfiguration : IEntityTypeConfiguration<PunchItem>
{
    public void Configure(EntityTypeBuilder<PunchItem> builder)
    {
        builder.ConfigureSystemVersioning();
        builder.ConfigurePlant();
        builder.ConfigureCreationAudit();
        builder.ConfigureModificationAudit();
        builder.ConfigureConcurrencyToken();

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        builder.Property(x => x.Id)
            // Punch created in PCS5 has Id > 4000000. Punch created in PCS4 has Id <= 4000000
            .UseIdentityColumn(4000001);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(PunchItem.DescriptionLengthMax);

        builder
            .Property(x => x.ClearedAtUtc)
            .HasConversion(CompletionContext.DateTimeKindConverter);

        builder
            .HasOne<Person>()
            .WithMany()
            .HasForeignKey(x => x.ClearedById)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Property(x => x.RejectedAtUtc)
            .HasConversion(CompletionContext.DateTimeKindConverter);

        builder
            .HasOne<Person>()
            .WithMany()
            .HasForeignKey(x => x.RejectedById)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Property(x => x.VerifiedAtUtc)
            .HasConversion(CompletionContext.DateTimeKindConverter);

        builder
            .HasOne<Person>()
            .WithMany()
            .HasForeignKey(x => x.VerifiedById)
            .OnDelete(DeleteBehavior.NoAction);

        // both ClearedAtUtc and ClearedById fields must either be set or not set
        builder
            .ToTable(x => x.HasCheckConstraint("punch_item_check_cleared",
                $"({nameof(PunchItem.ClearedAtUtc)} is null and {nameof(PunchItem.ClearedById)} is null) or " +
                $"({nameof(PunchItem.ClearedAtUtc)} is not null and {nameof(PunchItem.ClearedById)} is not null)"));

        // both VerifiedAtUtc and VerifiedById fields must either be set or not set
        builder
            .ToTable(x => x.HasCheckConstraint("punch_item_check_verified",
                $"({nameof(PunchItem.VerifiedAtUtc)} is null and {nameof(PunchItem.VerifiedById)} is null) or " +
                $"({nameof(PunchItem.VerifiedAtUtc)} is not null and {nameof(PunchItem.VerifiedById)} is not null)"));

        // both RejectedAtUtc and RejectedById fields must either be set or not set
        builder
            .ToTable(x => x.HasCheckConstraint("punch_item_check_rejected",
                $"({nameof(PunchItem.RejectedAtUtc)} is null and {nameof(PunchItem.RejectedById)} is null) or " +
                $"({nameof(PunchItem.RejectedAtUtc)} is not null and {nameof(PunchItem.RejectedById)} is not null)"));

        // can't be both cleared and rejected at same time
        builder
            .ToTable(x => x.HasCheckConstraint("punch_item_check_cleared_rejected",
                $"not ({nameof(PunchItem.ClearedAtUtc)} is not null and {nameof(PunchItem.RejectedAtUtc)} is not null)"));

        // if verified, it also must be cleared
        builder
            .ToTable(x => x.HasCheckConstraint("punch_item_check_cleared_verified",
                $"not ({nameof(PunchItem.ClearedAtUtc)} is null and {nameof(PunchItem.VerifiedAtUtc)} is not null)"));

        builder
            .HasIndex(x => x.Guid)
            .HasDatabaseName("IX_PunchItems_Guid")
            .IncludeProperties(x => new
            {
                x.Id,
                x.Description,
                x.ProjectId,
                x.CreatedById,
                x.CreatedAtUtc,
                x.ModifiedById,
                x.ModifiedAtUtc,
                x.ClearedById,
                x.ClearedAtUtc,
                x.VerifiedById,
                x.VerifiedAtUtc,
                x.RejectedById,
                x.RejectedAtUtc,
                x.RowVersion
            });

        builder
            .HasIndex(x => x.ProjectId)
            .HasDatabaseName("IX_PunchItems_ProjectId")
            .IncludeProperties(x => new
            {
                x.Id,
                x.RowVersion
            });
    }
}
