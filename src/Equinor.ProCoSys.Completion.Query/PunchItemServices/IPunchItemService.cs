using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Query.PunchItemServices;

public interface IPunchItemService
{
    Task<PunchItemDetailsDto> GetByGuid(Guid guid, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PunchItemDetailsDto>> GetByCheckListGuid(Guid guid, CancellationToken cancellationToken);
}

