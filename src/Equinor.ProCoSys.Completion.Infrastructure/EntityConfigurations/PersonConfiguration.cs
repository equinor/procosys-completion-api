using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations;

internal class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ConfigureSystemVersioning();
        builder.ConfigureModificationAudit();
        builder.ConfigureConcurrencyToken();

        builder.Property(x => x.Guid)
            .IsRequired();

        builder.HasIndex(x => x.Guid)
            .IsUnique();

        builder.Property(x => x.Email)
            .HasMaxLength(Person.EmailLengthMax)
            .IsRequired();

        builder.Property(x => x.FirstName)
            .HasMaxLength(Person.FirstNameLengthMax)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(Person.LastNameLengthMax)
            .IsRequired();

        builder.Property(x => x.UserName)
            .HasMaxLength(Person.UserNameLengthMax)
            .IsRequired();
    }
}
