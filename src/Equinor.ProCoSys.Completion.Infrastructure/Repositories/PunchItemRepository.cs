using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class PunchItemRepository(CompletionContext context) : EntityWithGuidRepository<PunchItem>(context,
    context.PunchItems,
    context.PunchItems
        .Include(p => p.Project)
        .Include(p => p.CreatedBy)
        .Include(p => p.ModifiedBy)
        .Include(p => p.ClearedBy)
        .Include(p => p.RejectedBy)
        .Include(p => p.VerifiedBy)
        .Include(p => p.ActionBy)
        .Include(p => p.RaisedByOrg)
        .Include(p => p.ClearingByOrg)
        .Include(p => p.Priority)
        .Include(p => p.Sorting)
        .Include(p => p.Type)
        .Include(p => p.OriginalWorkOrder)
        .Include(p => p.WorkOrder)
        .Include(p => p.SWCR)
        .Include(p => p.Document)
    ), IPunchItemRepository
{
    public async Task<List<Guid>> GetAllUniqueCheckListGuidsAsync(CancellationToken cancellationToken)
    {
        var checkListGuids =
            await Set
                .IgnoreQueryFilters()
                .Select(p => p.CheckListGuid).ToListAsync(cancellationToken);

        return checkListGuids.Distinct().ToList();
    }

    public async Task<List<PunchItem>> GetByCheckListGuidsAsync(List<Guid> checkListGuids, CancellationToken cancellationToken)
    {
        var punchItems =
            await Set.Include(p => p.Project)
                .IgnoreQueryFilters()
                .Where(p => checkListGuids.Contains(p.CheckListGuid))
                .ToListAsync(cancellationToken);

        return punchItems;
    }
}
