using System;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public interface IContentAccessChecker
{
    Task<bool> HasCurrentUserAccessToCheckListAsync(Guid checkListGuid);
    Task<bool> HasCurrentUserAccessToCheckListOwningPunchItemAsync(Guid punchItemGuid);
}
