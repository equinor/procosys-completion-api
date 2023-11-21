using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelHostAggregate;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class LabelHostRepository : EntityRepository<LabelHost>, ILabelHostRepository
{
    public LabelHostRepository(CompletionContext context)
        : base(context, context.LabelHosts)
    {
    }
}
