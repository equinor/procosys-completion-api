using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations;

internal class LabelConfiguration : IEntityTypeConfiguration<Label>
{
    public void Configure(EntityTypeBuilder<Label> builder)
    {
        builder.ConfigureSystemVersioning();
        builder.ConfigureCreationAudit();
        builder.ConfigureModificationAudit();
        builder.ConfigureConcurrencyToken();

        builder
            // MS SQL is case insensitive, which will prohibit adding same labels with different casing
            .HasIndex(x => x.Text)
            .IsUnique();

        builder.Property(x => x.Text)
            .HasMaxLength(Label.TextLengthMax)
            .IsRequired();
    }
}
