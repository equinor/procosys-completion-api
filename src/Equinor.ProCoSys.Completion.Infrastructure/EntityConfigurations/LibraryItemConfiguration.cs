﻿using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations;

internal class LibraryItemConfiguration : IEntityTypeConfiguration<LibraryItem>
{
    public void Configure(EntityTypeBuilder<LibraryItem> builder)
    {
        builder.ConfigureSystemVersioning();
        builder.ConfigurePlant();
        builder.ConfigureConcurrencyToken();
        
        builder.Property(x => x.Code)
            .HasMaxLength(LibraryItem.CodeLengthMax)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(LibraryItem.DescriptionLengthMax)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion(CompletionContext.LibraryTypeConverter)
            .HasMaxLength(LibraryItem.TypeLengthMax)
            .IsRequired();

        builder
            .HasMany(x => x.Classifications)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(x => x.Guid)
            .IsUnique()
            .HasDatabaseName("IX_LibraryItems_Guid")
            .IncludeProperties(x => new
            {
                x.Code,
                x.Description,
                x.Type
            });
    }
}
