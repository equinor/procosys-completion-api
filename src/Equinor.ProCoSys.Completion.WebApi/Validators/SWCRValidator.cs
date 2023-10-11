using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.WebApi.Validators;

public class SWCRValidator : ISWCRValidator
{
    private readonly IReadOnlyContext _context;

    public SWCRValidator(IReadOnlyContext context) => _context = context;

    public async Task<bool> ExistsAsync(Guid swcrGuid, CancellationToken cancellationToken) =>
        await (from l in _context.QuerySet<SWCR>()
            where l.Guid == swcrGuid
            select l).AnyAsync(cancellationToken);

    public async Task<bool> IsVoidedAsync(Guid swcrGuid, CancellationToken cancellationToken)
    {
        var swcr = await (from s in _context.QuerySet<SWCR>()
            where s.Guid == swcrGuid
            select s).SingleOrDefaultAsync(cancellationToken);

        return swcr is not null && swcr.IsVoided;
    }
}
