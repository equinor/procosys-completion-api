using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class HistoryItemRepository : EntityWithGuidRepository<HistoryItem>, IHistoryItemRepository
{
    public HistoryItemRepository(CompletionContext context)
        : base(context, context.History, context.History.Include(h => h.Properties))
    {
    }
}
