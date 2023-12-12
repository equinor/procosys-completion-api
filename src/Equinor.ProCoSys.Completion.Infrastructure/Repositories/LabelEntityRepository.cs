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

    public async Task<bool> ExistsAsync(EntityWithLabelType entityWithLabelType, CancellationToken cancellationToken)
        => await Set.AnyAsync(e => e.EntityWithLabel == entityWithLabelType, cancellationToken);
}
