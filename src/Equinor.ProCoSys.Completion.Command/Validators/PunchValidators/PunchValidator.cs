using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Microsoft.EntityFrameworkCore;
using Equinor.ProCoSys.Common;
using System;

namespace Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;

public class PunchValidator : IPunchValidator
{
    private readonly IReadOnlyContext _context;

    public PunchValidator(IReadOnlyContext context) => _context = context;

    public async Task<bool> PunchExistsAsync(Guid punchGuid, CancellationToken cancellationToken) =>
        await (from f in _context.QuerySet<Punch>()
            where f.Guid == punchGuid
            select f).AnyAsync(cancellationToken);

    public async Task<bool> PunchIsVoidedAsync(Guid punchGuid, CancellationToken cancellationToken)
    {
        var punch = await (from f in _context.QuerySet<Punch>()
            where f.Guid == punchGuid
            select f).SingleOrDefaultAsync(cancellationToken);
        return punch != null && punch.IsVoided;
    }
}
