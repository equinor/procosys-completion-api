using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Command.Validators;

public class SWCRValidator : ISWCRValidator
{
    private readonly IReadOnlyContext _context;

    // Trick to write LINQ queries to let EF create effective SQL queries is
    // 1) use Any
    // 2) select a projection with as few columns as needed
    public SWCRValidator(IReadOnlyContext context) => _context = context;

    public async Task<bool> ExistsAsync(Guid swcrGuid, CancellationToken cancellationToken) =>
        await (from l in _context.QuerySet<SWCR>()
            where l.Guid == swcrGuid
            select 1).AnyAsync(cancellationToken);

    public async Task<bool> IsVoidedAsync(Guid swcrGuid, CancellationToken cancellationToken)
        => await (from s in _context.QuerySet<SWCR>()
            where s.Guid == swcrGuid && s.IsVoided == true
            select 1).AnyAsync(cancellationToken);
}
