using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class PunchItemRepository : EntityWithGuidRepository<PunchItem>, IPunchItemRepository
{
    public PunchItemRepository(CompletionContext context)
        : base(context, context.PunchItems)
    {
    }
}
