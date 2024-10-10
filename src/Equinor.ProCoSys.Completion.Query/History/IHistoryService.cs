using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Query.History;

public interface IHistoryService
{
    Task<IReadOnlyCollection<HistoryDto>> GetAllAsync(Guid parentGuid,
        CancellationToken cancellationToken);
}
