using System;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations;

internal class LabelEntityConfiguration : IEntityTypeConfiguration<LabelEntity>
{
    public void Configure(EntityTypeBuilder<LabelEntity> builder)
    {
        builder.ConfigureSystemVersioning();
        builder.ConfigureCreationAudit();
        builder.ConfigureModificationAudit();
        builder.ConfigureConcurrencyToken();

        builder
            .HasIndex(x => x.EntityWithLabel)
            .IsUnique();

        builder.Property(f => f.EntityWithLabel)
            .HasConversion<string>()
            .HasDefaultValue(EntityWithLabelType.PunchPicture)
            .IsRequired();

        builder
            .ToTable(x => x.HasCheckConstraint("valid_entity_type",
                $"{nameof(LabelEntity.EntityWithLabel)} in ({GetValidEntityWithLabelTypeEnums()})"));

        builder
            .HasMany(x => x.Labels)
            .WithMany(x => x.EntitiesWithLabel);
    }

    private string GetValidEntityWithLabelTypeEnums()
    {
        var names = Enum.GetNames(typeof(EntityWithLabelType)).Select(t => $"'{t}'");
        return string.Join(',', names);
    }
}
