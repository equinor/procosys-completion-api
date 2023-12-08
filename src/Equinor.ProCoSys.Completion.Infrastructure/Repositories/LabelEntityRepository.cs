using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class LabelEntityRepository : EntityRepository<LabelEntity>, ILabelEntityRepository
{
    public LabelEntityRepository(CompletionContext context)
        : base(context, context.LabelEntities)
    {
    }
}
