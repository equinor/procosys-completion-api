using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class SWCRRepository : EntityWithGuidRepository<SWCR>, ISWCRRepository
{
    public SWCRRepository(CompletionContext context)
        : base(context, context.SWCRs)
            
    {
    }
}
