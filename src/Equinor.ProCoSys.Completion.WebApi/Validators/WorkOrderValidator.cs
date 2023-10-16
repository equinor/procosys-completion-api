using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.WebApi.Validators;

public class WorkOrderValidator : IWorkOrderValidator
{
    private readonly IReadOnlyContext _context;

    public WorkOrderValidator(IReadOnlyContext context) => _context = context;

    public async Task<bool> ExistsAsync(Guid workOrderGuid, CancellationToken cancellationToken) =>
        await (from l in _context.QuerySet<WorkOrder>()
            where l.Guid == workOrderGuid
            select l).AnyAsync(cancellationToken);

    public async Task<bool> IsClosedAsync(Guid workOrderGuid, CancellationToken cancellationToken)
    {
        var workOrder = await (from wo in _context.QuerySet<WorkOrder>()
            where wo.Guid == workOrderGuid
            select wo).SingleOrDefaultAsync(cancellationToken);

        return workOrder is not null && workOrder.IsClosed;
    }
}
