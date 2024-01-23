using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations;

internal class LinkConfiguration : IEntityTypeConfiguration<Link>
{
    public void Configure(EntityTypeBuilder<Link> builder)
    {
        builder.ConfigureSystemVersioning();
        builder.ConfigureCreationAudit();
        builder.ConfigureModificationAudit();
        builder.ConfigureConcurrencyToken();

        builder
            .HasIndex(x => x.Guid)
            .IsUnique();

        builder.Property(x => x.ParentType)
            .HasMaxLength(Link.ParentTypeLengthMax)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(Link.TitleLengthMax)
            .IsRequired();

        builder.Property(x => x.Url)
            .HasMaxLength(Link.UrlLengthMax)
            .IsRequired();

        builder
            .HasIndex(x => x.ParentGuid)
            .HasDatabaseName("IX_Links_ParentGuid")
            .IncludeProperties(x => new
            {
                x.Guid,
                x.Url,
                x.Title,
                x.RowVersion
            });
    }
}
