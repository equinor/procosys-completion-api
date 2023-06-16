using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations;

internal class PunchConfiguration : IEntityTypeConfiguration<Punch>
{
    public void Configure(EntityTypeBuilder<Punch> builder)
    {
        builder.ConfigurePlant();
        builder.ConfigureCreationAudit();
        builder.ConfigureModificationAudit();
        builder.ConfigureConcurrencyToken();

        builder.ToTable(t => t.IsTemporal());

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        builder.Property(x => x.Title)
            .HasMaxLength(Punch.TitleLengthMax)
            .IsRequired();

        builder.Property(x => x.Text)
            .HasMaxLength(Punch.TextLengthMax);

        builder
            .HasIndex(x => x.Guid)
            .HasDatabaseName("IX_Punch_Guid")
            .IncludeProperties(x => new
            {
                x.Title,
                x.Text,
                x.ProjectId,
                x.CreatedById,
                x.CreatedAtUtc,
                x.ModifiedById,
                x.ModifiedAtUtc,
                x.IsVoided,
                x.RowVersion
            });

        builder
            .HasIndex(x => x.ProjectId)
            .HasDatabaseName("IX_Punch_ProjectId")
            .IncludeProperties(x => new
            {
                x.Title,
                x.IsVoided,
                x.RowVersion
            });
    }
}
