using System.Collections.Generic;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public class AccessChecker(IRestrictionRolesChecker restrictionRolesChecker) : IAccessChecker
{
    public bool HasCurrentUserWriteAccessToCheckList(CheckListDetailsDto checkListDetailsDto)
    {
        if (restrictionRolesChecker.HasCurrentUserExplicitNoRestrictions())
        {
            return true;
        }
        return restrictionRolesChecker.HasCurrentUserExplicitAccessToContent(checkListDetailsDto.ResponsibleCode);
    }

    // todo unit tests
    public bool HasCurrentUserWriteAccessToAllCheckLists(List<CheckListDetailsDto> checkListDetailsDtos)
    {
        if (restrictionRolesChecker.HasCurrentUserExplicitNoRestrictions())
        {
            return true;
        }

        foreach (var checkListDetailsDto in checkListDetailsDtos)
        {
            if (!restrictionRolesChecker.HasCurrentUserExplicitAccessToContent(checkListDetailsDto.ResponsibleCode))
            {
                return false;
            }
        }

        return true;
    }
}
