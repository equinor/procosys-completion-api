using System;
using System.Linq;
using System.Security.Claims;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Auth.Misc;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public class ProjectAccessChecker : IProjectAccessChecker
{
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;

    public ProjectAccessChecker(IClaimsPrincipalProvider claimsPrincipalProvider) => _claimsPrincipalProvider = claimsPrincipalProvider;

    public bool HasCurrentUserAccessToProject(Guid projectGuid)
    {
        var userDataClaimWithProject = ClaimsTransformation.GetProjectClaimValue(projectGuid);
        //TODO: JSOI Get claim not from HttpContext but from other source...
        //return _claimsPrincipalProvider.GetCurrentClaimsPrincipal().Claims.Any(c => c.Type == ClaimTypes.UserData && c.Value == userDataClaimWithProject);
        return true;
    }
}
