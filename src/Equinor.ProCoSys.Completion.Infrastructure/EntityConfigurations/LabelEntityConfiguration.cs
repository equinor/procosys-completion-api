﻿using System;
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
            .HasIndex(x => x.EntityType)
            .IsUnique();

        builder.Property(f => f.EntityType)
            .HasConversion<string>()
            .HasDefaultValue(EntityTypeWithLabel.PunchPicture)
            .IsRequired();

        builder
            .ToTable(x => x.HasCheckConstraint("valid_entity_type",
                $"{nameof(LabelEntity.EntityType)} in ({GetValidEntityTypeWithLabelEnums()})"));
    }

    private string GetValidEntityTypeWithLabelEnums()
    {
        var names = Enum.GetNames(typeof(EntityTypeWithLabel)).Select(t => $"'{t}'");
        return string.Join(',', names);
    }
}
