using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Query.History;

public class HistoryService : IHistoryService
{
    private readonly IReadOnlyContext _context;

    public HistoryService(IReadOnlyContext context) => _context = context;

    public async Task<IEnumerable<HistoryDto>> GetAllAsync(
        Guid guid,
        CancellationToken cancellationToken)
    {
        var historyItems =
            await (from c in _context.QuerySet<HistoryItem>()
                        .Include(c => c.Properties)
                    where c.EventForGuid == guid
                    select c)
                .TagWith($"{nameof(HistoryService)}.{nameof(GetAllAsync)}")
                .ToListAsync(cancellationToken);

        var historyDtos = historyItems.Select(c => new HistoryDto(
            c.EventForParentGuid,
            c.EventForGuid,
            c.EventByOid,
            c.EventAtUtc,
            c.EventDisplayName,
            c.EventByFullName, 
            c.Properties.Select(p => new PropertyDto(
                p.Name,
                p.OldValue,
                p.CurrentValue,
                p.ValueDisplayType
            )).ToList(),
            c.RowVersion.ConvertToString()));

        return historyDtos;
    }
}
