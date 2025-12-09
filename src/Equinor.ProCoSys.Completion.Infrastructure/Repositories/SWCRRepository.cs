using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class SWCRRepository(CompletionContext context) : EntityWithGuidRepository<SWCR>(context, context.SWCRs), ISWCRRepository
{
    public async Task<SWCR?> GetByNoAsync(int swcrNo, CancellationToken cancellationToken) => await DefaultQueryable
            .SingleOrDefaultAsync(s => s.No == swcrNo, cancellationToken);
}
