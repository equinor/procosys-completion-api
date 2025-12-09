using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

public interface IPunchItemRepository : IRepositoryWithGuid<PunchItem>
{
    Task<PunchItem?> GetByItemNoAsync(long itemNo, Guid checklistGuid, CancellationToken cancellationToken);
    Task<PunchItem?> GetByExternalItemNoAsync(string externalItemNo, Guid checklistGuid, CancellationToken cancellationToken);
}
