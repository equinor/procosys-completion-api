using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations;

internal class HistoryItemConfiguration : IEntityTypeConfiguration<HistoryItem>
{
    public void Configure(EntityTypeBuilder<HistoryItem> builder)
    {
        builder.ConfigureSystemVersioning();

        builder.Property(x => x.EventForGuid)
            .IsRequired();

        builder.Property(x => x.EventAtUtc)
            .IsRequired();

        builder.Property(x => x.EventDisplayName)
            .HasMaxLength(HistoryItem.EventDisplayNameLengthMax)
            .IsRequired();

        builder.Property(x => x.EventByFullName)
            .HasMaxLength(HistoryItem.EventByFullNameLengthMax)
            .IsRequired();

        builder
            .Property(x => x.EventAtUtc)
            .HasConversion(CompletionContext.DateTimeKindConverter);

        builder
            .HasMany(x => x.Properties)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        // todo History: Create index

        builder
            .HasIndex(x => x.Guid)
            .IsUnique();

        builder
            .HasIndex(x => x.EventForGuid);
        
        builder
            .HasIndex(x => x.EventForParentGuid);

        builder
            .HasIndex(x => x.EventByOid);
    }
}
