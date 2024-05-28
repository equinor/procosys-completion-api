using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Command.Validators;

public class WorkOrderValidator(IReadOnlyContext context) : IWorkOrderValidator
{
    public async Task<bool> ExistsAsync(Guid workOrderGuid, CancellationToken cancellationToken)
    {
        var workOrder = await GetWorkOrderAsync(workOrderGuid, cancellationToken);
        return workOrder is not null;
    }

    public async Task<bool> IsVoidedAsync(Guid workOrderGuid, CancellationToken cancellationToken)
    {
        var workOrder = await GetWorkOrderAsync(workOrderGuid, cancellationToken);
        return workOrder is not null && workOrder.IsVoided;
    }

    private async Task<WorkOrder?> GetWorkOrderAsync(Guid workOrderGuid, CancellationToken cancellationToken)
        => await (from wo in context.QuerySet<WorkOrder>()
            where wo.Guid == workOrderGuid
            select wo).SingleOrDefaultAsync(cancellationToken);
}
