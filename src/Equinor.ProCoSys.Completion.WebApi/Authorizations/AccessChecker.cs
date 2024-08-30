using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public class AccessChecker(IRestrictionRolesChecker restrictionRolesChecker) : IAccessChecker
{
    public bool HasCurrentUserWriteAccessToCheckList(CheckListDetailsDto checkListDetailsDto)
        => restrictionRolesChecker.HasCurrentUserExplicitNoRestrictions() ||
           restrictionRolesChecker.HasCurrentUserExplicitAccessToContent(checkListDetailsDto.ResponsibleCode);

    public bool HasCurrentUserWriteAccessToAllCheckLists(List<CheckListDetailsDto> checkListDetailsDtos)
        => restrictionRolesChecker.HasCurrentUserExplicitNoRestrictions() ||
           checkListDetailsDtos.All(dto =>
               restrictionRolesChecker.HasCurrentUserExplicitAccessToContent(dto.ResponsibleCode));
}
