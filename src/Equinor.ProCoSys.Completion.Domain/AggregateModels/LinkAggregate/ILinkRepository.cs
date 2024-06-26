using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;

public interface ILinkRepository : IRepositoryWithGuid<Link>
{
    Task<IEnumerable<Link>> GetAllByParentGuidAsync(Guid requestPunchItemGuid, CancellationToken cancellationToken);
}
