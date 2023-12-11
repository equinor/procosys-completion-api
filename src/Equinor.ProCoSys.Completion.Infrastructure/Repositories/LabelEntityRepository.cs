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

    public Task<LabelEntity> GetByTypeAsync(EntityTypeWithLabels entityType, CancellationToken cancellationToken)
        => DefaultQuery.Where(e => e.EntityType == entityType).SingleAsync(cancellationToken);
}
