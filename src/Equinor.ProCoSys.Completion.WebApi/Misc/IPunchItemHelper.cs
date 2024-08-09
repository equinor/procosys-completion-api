using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.WebApi.Misc;

public interface IPunchItemHelper
{
    Task<Guid?> GetProjectGuidForPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken);
    Task<Guid?> GetCheckListGuidForPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken);
}
