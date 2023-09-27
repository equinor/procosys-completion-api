using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations;

internal class SWCRConfiguration : IEntityTypeConfiguration<SWCR>
{
    public void Configure(EntityTypeBuilder<SWCR> builder)
    {
        builder.ConfigureSystemVersioning();
        builder.ConfigurePlant();
        builder.ConfigureCreationAudit();
        builder.ConfigureModificationAudit();
        builder.ConfigureConcurrencyToken();

        builder
            .HasIndex(x => x.Guid)
            .IsUnique();
    }
}
