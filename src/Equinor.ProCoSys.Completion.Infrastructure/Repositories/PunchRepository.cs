using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class PunchRepository : EntityWithGuidRepository<Punch>, IPunchRepository
{
    public PunchRepository(CompletionContext context)
        : base(context, context.Punches)
    {
    }
}
