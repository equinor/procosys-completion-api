using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations;

internal class ClassificationConfiguration : IEntityTypeConfiguration<Classification>
{
    public void Configure(EntityTypeBuilder<Classification> builder)
    {
        builder.ConfigureSystemVersioning();
        builder.Property(x => x.Guid).ValueGeneratedNever();
        builder.HasIndex("Name","LibraryItemId").IsUnique();
        builder.HasKey(x => x.Guid);
        builder.Property(x => x.Name)
            .HasMaxLength(Classification.NameLengthMax)
            .IsRequired();
    }
}
