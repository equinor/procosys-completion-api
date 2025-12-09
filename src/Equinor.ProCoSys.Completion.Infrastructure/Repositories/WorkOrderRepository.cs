using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class WorkOrderRepository(CompletionContext context) : EntityWithGuidRepository<WorkOrder>(context, context.WorkOrders), IWorkOrderRepository
{
    public async Task<WorkOrder?> GetByNoAsync(string no, CancellationToken cancellationToken) => await DefaultQueryable
            .SingleOrDefaultAsync(w => w.No == no, cancellationToken);
}
