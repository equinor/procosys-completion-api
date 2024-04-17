using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.SWCRQueries;

public class SWCRSearchQueryHandler : IRequestHandler<SWCRSearchQuery, Result<IEnumerable<SWCRDto>>>
{
    private readonly IReadOnlyContext _context;

    public SWCRSearchQueryHandler(IReadOnlyContext context) => _context = context;

    public async Task<Result<IEnumerable<SWCRDto>>> Handle(SWCRSearchQuery request, CancellationToken cancellationToken)
    {
        var swcrs = await (from swcr in _context.QuerySet<SWCR>()
                where swcr.No.ToString().Contains(request.SearchPhrase)
                select new SWCRDto(
                    swcr.Guid,
                    swcr.No
                )).TagWith($"{nameof(SWCRSearchQueryHandler)}.{nameof(Handle)}")
            .ToListAsync(cancellationToken);

        var orderedSWCRs = swcrs.OrderBy(l => l.No);

        return new SuccessResult<IEnumerable<SWCRDto>>(orderedSWCRs);
    }
}
