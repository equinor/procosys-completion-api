using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class PunchItemRepository : EntityWithGuidRepository<PunchItem>, IPunchItemRepository
{
    public PunchItemRepository(CompletionContext context)
        : base(context, 
            context.PunchItems,
            context.PunchItems
                .Include(p => p.Project)
                .Include(p => p.CreatedBy)
                .Include(p => p.ModifiedBy)
                .Include(p => p.ClearedBy)
                .Include(p => p.RejectedBy)
                .Include(p => p.VerifiedBy)
                .Include(p => p.RaisedByOrg)
                .Include(p => p.ClearingByOrg)
                .Include(p => p.Priority)
                .Include(p => p.Sorting)
                .Include(p => p.Type))
    {
    }
}
