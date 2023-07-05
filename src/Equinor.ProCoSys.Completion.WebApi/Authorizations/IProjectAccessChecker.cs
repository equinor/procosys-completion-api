using System;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public interface IProjectAccessChecker
{
    bool HasCurrentUserAccessToProject(Guid projectGuid);
}
