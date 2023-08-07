using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations;

internal class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
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

        builder.Property(x => x.SourceType)
            .HasMaxLength(Attachment.SourceTypeLengthMax)
            .IsRequired();

        builder.Property(x => x.FileName)
            .HasMaxLength(Attachment.FileNameLengthMax)
            .IsRequired();

        builder.Property(x => x.BlobPath)
            .HasMaxLength(Attachment.BlobPathLengthMax)
            .IsRequired();

        builder
            .HasIndex(x => x.SourceGuid)
            .HasDatabaseName("IX_Attachments_SourceGuid")
            .IncludeProperties(x => new
            {
                x.Guid,
                x.FileName,
                x.CreatedById,
                x.CreatedAtUtc,
                x.ModifiedById,
                x.ModifiedAtUtc,
                x.RowVersion
            });
    }
}
