using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public interface IAccessChecker
{
    bool HasCurrentUserWriteAccessToCheckList(CheckListDetailsDto checkListDetailsDto);
    bool HasCurrentUserWriteAccessToAllCheckLists(List<CheckListDetailsDto> checkListDetailsDtos);
}
