using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;

public interface IWorkOrderRepository : IRepositoryWithGuid<WorkOrder>
{
    Task<WorkOrder?> GetByWoNoAsync(string woNo, CancellationToken cancellationToken);
}
