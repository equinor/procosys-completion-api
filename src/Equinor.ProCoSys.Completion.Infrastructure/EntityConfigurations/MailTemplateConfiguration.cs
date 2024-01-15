using Equinor.ProCoSys.Completion.Domain.AggregateModels.MailTemplateAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations;

internal class MailTemplateConfiguration : IEntityTypeConfiguration<MailTemplate>
{
    public void Configure(EntityTypeBuilder<MailTemplate> builder)
    {
        builder.ConfigureSystemVersioning();
        builder.ConfigureCreationAudit();
        builder.ConfigureModificationAudit();
        builder.ConfigureConcurrencyToken();

        builder.ToTable(t => t.IsTemporal());

        builder.Property(x => x.Code)
            .HasMaxLength(MailTemplate.CodeLengthMax)
            .IsRequired();

        builder.Property(x => x.Subject)
            .HasMaxLength(MailTemplate.SubjectLengthMax)
            .IsRequired();

        builder.Property(x => x.Body)
            .HasMaxLength(MailTemplate.BodyLengthMax)
            .IsRequired();

        builder.Property(x => x.Plant)
            .HasMaxLength(MailTemplate.PlantLengthMax);

        builder
            .HasIndex(x => new
            {
                x.Code,
                x.Plant
            })
            .IsUnique()
            .HasDatabaseName("IX_MailTemplates_Code")
            .IncludeProperties(x => new
            {
                x.Subject,
                x.Body,
                x.IsVoided
            });
    }
}
