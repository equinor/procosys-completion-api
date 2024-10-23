using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

public interface IPunchItemRepository : IRepositoryWithGuid<PunchItem>
{
    Task<List<Guid>> GetAllUniqueCheckListGuidsAsync(CancellationToken cancellationToken);
    Task<List<PunchItem>> GetByCheckListGuidsAsync(List<Guid> checkListGuids, CancellationToken cancellationToken);
}
