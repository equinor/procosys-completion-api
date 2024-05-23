using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Query.History;

public class HistoryService : IHistoryService
{
    private readonly IReadOnlyContext _context;

    public HistoryService(IReadOnlyContext context) => _context = context;

    public async Task<IEnumerable<HistoryDto>> GetAllAsync(
        Guid parentGuid,
        CancellationToken cancellationToken)
    {
       
        return await Task.FromResult(new List<HistoryDto>());
    }
}
