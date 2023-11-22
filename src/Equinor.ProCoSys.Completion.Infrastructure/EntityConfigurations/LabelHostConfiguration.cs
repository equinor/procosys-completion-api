using System;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelHostAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations;

internal class LabelHostConfiguration : IEntityTypeConfiguration<LabelHost>
{
    public void Configure(EntityTypeBuilder<LabelHost> builder)
    {
        builder.ConfigureSystemVersioning();
        builder.ConfigureCreationAudit();
        builder.ConfigureModificationAudit();
        builder.ConfigureConcurrencyToken();

        builder
            .HasIndex(x => x.Type)
            .IsUnique();

        builder.Property(f => f.Type)
            .HasConversion<string>()
            .HasDefaultValue(HostType.GeneralPicture)
            .IsRequired();

        builder
            .ToTable(x => x.HasCheckConstraint("host_type_valid_type",
                $"{nameof(LabelHost.Type)} in ({GetValidHostTypeEnums()})"));

        builder
            .HasMany(x => x.Labels)
            .WithMany(x => x.Hosts);
    }

    private string GetValidHostTypeEnums()
    {
        var names = Enum.GetNames(typeof(HostType)).Select(t => $"'{t}'");
        return string.Join(',', names);
    }
}
