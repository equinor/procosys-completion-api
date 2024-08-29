using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Equinor.ProCoSys.Common;
using ServiceResult;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.WorkOrderQueries;

public class WorkOrderSearchQueryHandler(IReadOnlyContext context)
    : IRequestHandler<WorkOrderSearchQuery, Result<IEnumerable<WorkOrderDto>>>
{
    public async Task<Result<IEnumerable<WorkOrderDto>>> Handle(WorkOrderSearchQuery request, CancellationToken cancellationToken)
    {
        var workOrders = await (from workOrder in context.QuerySet<WorkOrder>()
            where workOrder.No.Contains(request.SearchPhrase) && !workOrder.IsVoided
            select new WorkOrderDto(
                workOrder.Guid,
                workOrder.No
            )).TagWith($"{nameof(WorkOrderSearchQueryHandler)}.{nameof(Handle)}")
            .ToListAsync(cancellationToken);

        var orderedWorkOrders = workOrders.OrderBy(l => l.No);

        return new SuccessResult<IEnumerable<WorkOrderDto>>(orderedWorkOrders);
    }
}
