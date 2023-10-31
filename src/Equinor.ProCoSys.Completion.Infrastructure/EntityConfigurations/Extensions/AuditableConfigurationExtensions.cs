using Equinor.ProCoSys.Completion.Domain.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.ProCoSys.Completion.Infrastructure.EntityConfigurations.Extensions;

public static class AuditableConfigurationExtensions
{
    public static void ConfigureCreationAudit<TEntity>(this EntityTypeBuilder<TEntity> builder) where TEntity : class, ICreationAuditable
    {
        builder
            .Property(x => x.CreatedAtUtc)
            .HasConversion(CompletionContext.DateTimeKindConverter);

        builder.HasOne(x => x.CreatedBy)
            .WithMany()
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
    }

    public static void ConfigureModificationAudit<TEntity>(this EntityTypeBuilder<TEntity> builder) where TEntity : class, IModificationAuditable
    {
        builder
            .Property(x => x.ModifiedAtUtc)
            .HasConversion(CompletionContext.DateTimeKindConverter);

        builder.HasOne(x => x.ModifiedBy)
            .WithMany()
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
