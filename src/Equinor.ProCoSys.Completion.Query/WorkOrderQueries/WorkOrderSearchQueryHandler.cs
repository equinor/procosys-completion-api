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

public class WorkOrderSearchQueryHandler : IRequestHandler<WorkOrderSearchQuery, Result<IEnumerable<WorkOrderDto>>>
{
    private readonly IReadOnlyContext _context;

    public WorkOrderSearchQueryHandler(IReadOnlyContext context) => _context = context;

    public async Task<Result<IEnumerable<WorkOrderDto>>> Handle(WorkOrderSearchQuery request, CancellationToken cancellationToken)
    {
        var workOrders = await (from workOrder in _context.QuerySet<WorkOrder>()
            where workOrder.No.Contains(request.SearchPhrase)
            where workOrder.Plant.Equals(request.Plant)
            select new WorkOrderDto(
                workOrder.Guid,
                workOrder.No
            )).TagWith($"{nameof(WorkOrderSearchQueryHandler)}.{nameof(Handle)}")
            .ToListAsync(cancellationToken);

        var orderedWorkOrders = workOrders.OrderBy(l => l.No);

        return new SuccessResult<IEnumerable<WorkOrderDto>>(orderedWorkOrders);
    }
}
