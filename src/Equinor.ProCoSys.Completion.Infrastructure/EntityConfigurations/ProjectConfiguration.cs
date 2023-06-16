using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations;

internal class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ConfigurePlant();
        builder.ConfigureCreationAudit();
        builder.ConfigureModificationAudit();
        builder.ConfigureConcurrencyToken();

        builder.Property(x => x.Name)
            .HasMaxLength(Project.NameLengthMax)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(Project.DescriptionLengthMax)
            .IsRequired();

        builder
            .HasIndex(x => x.Plant)
            .HasDatabaseName("IX_Projects_Plant_ASC")
            .IncludeProperties(x => new { x.Name, x.IsClosed, x.CreatedAtUtc, x.ModifiedAtUtc });

        builder
            .HasIndex(x => x.Name)
            .HasDatabaseName("IX_Projects_Name_ASC")
            .IncludeProperties(x => new { x.Plant });
    }
}
