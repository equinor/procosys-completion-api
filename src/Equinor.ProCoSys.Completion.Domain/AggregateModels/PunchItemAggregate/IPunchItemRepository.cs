using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

public interface IPunchItemRepository : IRepositoryWithGuid<PunchItem>
{
    Task<Project> GetProjectAsync(Guid punchItemGuid, CancellationToken cancellationToken);
}
