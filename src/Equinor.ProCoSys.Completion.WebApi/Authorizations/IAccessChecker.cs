using System;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public interface IAccessChecker
{
    Task<bool> HasCurrentUserWriteAccessToCheckListAsync(Guid checkListGuid);
    Task<bool> HasCurrentUserReadAccessToCheckListAsync(Guid checkListGuid);
    Task<bool> HasCurrentUserAccessToCheckListOwningPunchItemAsync(Guid punchItemGuid);
}
