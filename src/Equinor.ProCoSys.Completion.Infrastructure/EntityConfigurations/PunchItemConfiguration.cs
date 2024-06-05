using System;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
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

        builder.HasOne(x => x.Project)
            .WithMany()
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.RaisedByOrg)
            .WithMany()
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.ClearingByOrg)
            .WithMany()
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Sorting)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Type)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Priority)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.WorkOrder)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.OriginalWorkOrder)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Document)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.SWCR)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.ActionBy)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        builder.Property(x => x.Id)
            // Punch created in PCS5 has Id > 4000000. Punch created in PCS4 has Id <= 4000000
            .UseIdentityColumn(PunchItem.IdentitySeed);

        builder.Property(x => x.ItemNo)
            .HasDefaultValueSql($"NEXT VALUE FOR {PunchItem.PunchItemItemNoSequence}");
        builder.HasIndex(x => x.ItemNo).IsUnique();

        builder.Property(f => f.Category)
            .HasDefaultValue(Category.PA)
            .IsRequired();

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(PunchItem.DescriptionLengthMax);

        builder.Property(f => f.MaterialRequired)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.ExternalItemNo)
            .HasMaxLength(PunchItem.ExternalItemNoLengthMax);

        builder.Property(x => x.MaterialExternalNo)
            .HasMaxLength(PunchItem.MaterialExternalNoLengthMax);

        builder
            .Property(x => x.ClearedAtUtc)
            .HasConversion(CompletionContext.DateTimeKindConverter);

        builder.HasOne(x => x.ClearedBy)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Property(x => x.RejectedAtUtc)
            .HasConversion(CompletionContext.DateTimeKindConverter);

        builder.HasOne(x => x.RejectedBy)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Property(x => x.VerifiedAtUtc)
            .HasConversion(CompletionContext.DateTimeKindConverter);

        builder.HasOne(x => x.VerifiedBy)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Property(x => x.DueTimeUtc)
            .HasConversion(CompletionContext.DateTimeKindConverter);

        builder
            .Property(x => x.MaterialETAUtc)
            .HasConversion(CompletionContext.DateTimeKindConverter);

        builder
            .ToTable(x => x.HasCheckConstraint("punch_item_valid_category",
                $"{nameof(PunchItem.Category)} in ({GetValidCategoryEnums()})"));

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
            .IsUnique()
            .HasDatabaseName("IX_PunchItems_Guid")
            .IncludeProperties(x => new
            {
                x.Id,
                x.Category,
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
                x.RaisedByOrgId,
                x.ClearingByOrgId,
                x.SortingId,
                x.TypeId,
                x.PriorityId,
                x.Estimate,
                x.DueTimeUtc,
                x.ExternalItemNo,
                x.MaterialRequired,
                x.MaterialExternalNo,
                x.MaterialETAUtc,
                x.WorkOrderId,
                x.OriginalWorkOrderId,
                x.DocumentId,
                x.SWCRId,
                x.ActionById,
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

    private string GetValidCategoryEnums()
    {
        var values = Enum.GetValues(typeof(Category)).Cast<int>();
        return string.Join(',', values);
    }
}
