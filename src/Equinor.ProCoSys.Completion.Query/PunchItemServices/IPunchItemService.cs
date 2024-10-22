using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Query.PunchItemServices;

public interface IPunchItemService
{
    Task<PunchItemDetailsDto?> GetPunchItemOrNullByPunchItemGuidAsync(Guid punchItemGuid, CancellationToken cancellationToken);
    Task<IEnumerable<PunchItemTinyDetailsDto>> GetPunchItemsByPunchItemGuidsAsync(IEnumerable<Guid> punchItemGuids, CancellationToken cancellationToken);
    Task<ProjectDetailsDto?> GetProjectOrNullByPunchItemGuidAsync(Guid punchItemGuid, CancellationToken cancellationToken);
    Task<IEnumerable<PunchItemDetailsDto>> GetByCheckListGuid(Guid checkListGuid, CancellationToken cancellationToken);
}

