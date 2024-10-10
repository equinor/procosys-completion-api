using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations;

internal class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ConfigureSystemVersioning();
        builder.ConfigurePlant();
        builder.ConfigureConcurrencyToken();

        builder
            .HasIndex(x => x.Guid)
            .IsUnique();

        builder.Property(x => x.No)
            .HasMaxLength(Document.NoLengthMax)
            .IsRequired();
        
        // Document search performance
        builder.HasIndex(d => new { d.Plant, d.No, d.IsVoided });
    }
}
