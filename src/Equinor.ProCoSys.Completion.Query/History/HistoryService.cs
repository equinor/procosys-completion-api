﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Query.History;

public sealed class HistoryService(IReadOnlyContext context) : IHistoryService
{
    public async Task<IReadOnlyCollection<HistoryDto>> GetAllAsync(Guid parentGuid,
        CancellationToken cancellationToken)
    {
        var historyItems =
            await (from h in context.QuerySet<HistoryItem>()
                        .Include(h => h.Properties)
                    where h.EventForGuid == parentGuid || h.EventForParentGuid == parentGuid
                    orderby h.EventAtUtc descending 
                    select h)
                .TagWith($"{nameof(HistoryService)}.{nameof(GetAllAsync)}")
                .ToListAsync(cancellationToken);

        var historyDtos = historyItems.Select(h => new HistoryDto(
            h.EventForParentGuid,
            h.EventForGuid,
            h.EventByOid,
            h.EventAtUtc,
            h.EventDisplayName,
            h.EventByFullName, 
            h.Properties.Select(p => new PropertyDto(
                p.Name,
                p.OldValue,
                p.Value,
                p.ValueDisplayType
            )).ToList(),
            h.RowVersion.ConvertToString()));

        return historyDtos.ToArray();
    }
}
