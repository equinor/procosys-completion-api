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

    public SWCRValidator(IReadOnlyContext context) => _context = context;

    public async Task<bool> ExistsAsync(Guid swcrGuid, CancellationToken cancellationToken)
    {
        var swcr = await GetSWCRAsync(swcrGuid, cancellationToken);

        return swcr is not null;
    }

    public async Task<bool> IsVoidedAsync(Guid swcrGuid, CancellationToken cancellationToken)
    {
        var swcr = await GetSWCRAsync(swcrGuid, cancellationToken);

        return swcr is not null && swcr.IsVoided;
    }

    private async Task<SWCR?> GetSWCRAsync(Guid swcrGuid, CancellationToken cancellationToken)
        => await (from s in _context.QuerySet<SWCR>()
            where s.Guid == swcrGuid
            select s).SingleOrDefaultAsync(cancellationToken);
}
