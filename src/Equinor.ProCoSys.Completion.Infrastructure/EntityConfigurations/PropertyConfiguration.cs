using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations;

internal class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ConfigureSystemVersioning();

        builder.Property(x => x.Name)
            .HasMaxLength(Property.NameLengthMax)
            .IsRequired();

        builder.Property(x => x.ValueDisplayType)
            .HasMaxLength(Property.ValueDisplayTypeLengthMax)
            .IsRequired();

        // todo Create index
    }
}
