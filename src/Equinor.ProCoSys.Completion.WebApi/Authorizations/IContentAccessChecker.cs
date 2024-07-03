using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public interface IContentAccessChecker
{
    Task<bool> HasCurrentUserAccessToCheckListAsync(Guid checkListGuid, CancellationToken cancellationToken);
    Task<bool> HasCurrentUserAccessToCheckListOwningPunchItemAsync(Guid punchItemGuid,
        CancellationToken cancellationToken);
}
