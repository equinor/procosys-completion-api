using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class LabelEntityRepository : EntityRepository<LabelEntity>, ILabelEntityRepository
{
    public LabelEntityRepository(CompletionContext context)
        : base(context, context.LabelEntities)
    {
    }

    public async Task<LabelEntity> GetByTypeAsync(EntityTypeWithLabel entityType, CancellationToken cancellationToken)
    {
        var labelEntity = await DefaultQuery.Where(e => e.EntityType == entityType)
            .SingleOrDefaultAsync(cancellationToken);
        if (labelEntity == null)
        {
            throw new Exception($"Label entity {entityType} not found");
        }

        return labelEntity;
    }

    public async Task<bool> ExistsAsync(EntityTypeWithLabel entityType, CancellationToken cancellationToken)
        => await DefaultQuery.AnyAsync(e => e.EntityType == entityType, cancellationToken);
}
