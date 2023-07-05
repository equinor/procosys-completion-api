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

        builder.Property(x => x.ItemNo)
            .HasMaxLength(PunchItem.ItemNoLengthMax)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(PunchItem.DescriptionLengthMax);

        builder
            .HasIndex(x => x.Guid)
            .HasDatabaseName("IX_PunchItems_Guid")
            .IncludeProperties(x => new
            {
                x.ItemNo,
                x.Description,
                x.ProjectId,
                x.CreatedById,
                x.CreatedAtUtc,
                x.ModifiedById,
                x.ModifiedAtUtc,
                x.RowVersion
            });

        builder
            .HasIndex(x => x.ProjectId)
            .HasDatabaseName("IX_PunchItems_ProjectId")
            .IncludeProperties(x => new
            {
                x.ItemNo,
                x.RowVersion
            });
    }
}
