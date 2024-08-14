using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public interface IAccessChecker
{
    Task<bool> HasCurrentUserWriteAccessToCheckListAsync(Guid checkListGuid, CancellationToken cancellationToken);
}
