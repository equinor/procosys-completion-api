using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Query.PunchItemServices;

public interface IPunchItemService
{
    Task<PunchItemDetailsDto?> GetByGuid(Guid punchItemGuid, CancellationToken cancellationToken);
    Task<IEnumerable<PunchItemDetailsDto>> GetByCheckListGuid(Guid checkListGuid, CancellationToken cancellationToken);
}

