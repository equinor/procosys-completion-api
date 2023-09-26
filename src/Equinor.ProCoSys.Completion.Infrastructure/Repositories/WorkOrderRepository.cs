using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class WorkOrderRepository : EntityWithGuidRepository<WorkOrder>, IWorkOrderRepository
{
    public WorkOrderRepository(CompletionContext context)
        : base(context, context.WorkOrders)
            
    {
    }
}
