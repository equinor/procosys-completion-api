using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations;

internal class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ConfigureSystemVersioning();
        builder.ConfigureCreationAudit();
        builder.ConfigureConcurrencyToken();

        builder
            .HasIndex(x => x.Guid)
            .IsUnique();

        builder
            .Property(x => x.CreatedAtUtc)
            .HasConversion(CompletionContext.DateTimeKindConverter);

        builder.Property(x => x.ParentType)
            .HasMaxLength(Comment.ParentTypeLengthMax)
            .IsRequired();

        builder.Property(x => x.Text)
            .HasMaxLength(Comment.TextLengthMax)
            .IsRequired();

        builder
            .HasMany(x => x.Labels)
            .WithMany();

        builder
            .HasMany(x => x.Mentions)
            .WithMany();

        builder
            .HasIndex(x => x.ParentGuid)
            .HasDatabaseName("IX_Comments_ParentGuid")
            .IncludeProperties(x => new
            {
                x.Guid,
                x.Text,
                x.CreatedById,
                x.CreatedAtUtc
            });
    }
}
