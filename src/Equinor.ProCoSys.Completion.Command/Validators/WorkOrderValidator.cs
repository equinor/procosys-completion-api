using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Command.Validators;

public class WorkOrderValidator : IWorkOrderValidator
{
    private readonly IReadOnlyContext _context;

    // Trick to write LINQ queries to let EF create effective SQL queries is
    // 1) use Any
    // 2) select a projection with as few columns as needed
    public WorkOrderValidator(IReadOnlyContext context) => _context = context;

    public async Task<bool> ExistsAsync(Guid workOrderGuid, CancellationToken cancellationToken) =>
        await (from l in _context.QuerySet<WorkOrder>()
            where l.Guid == workOrderGuid
            select 1).AnyAsync(cancellationToken);

    public async Task<bool> IsClosedAsync(Guid workOrderGuid, CancellationToken cancellationToken)
        => await (from wo in _context.QuerySet<WorkOrder>()
            where wo.Guid == workOrderGuid && wo.IsClosed == true
            select 1).AnyAsync(cancellationToken);
}
