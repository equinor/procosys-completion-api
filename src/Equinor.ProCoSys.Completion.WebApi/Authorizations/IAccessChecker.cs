using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public interface IAccessChecker
{
    Task<bool> HasCurrentUserWriteAccessToCheckListAsync(Guid checkListGuid, CancellationToken cancellationToken);
    bool HasCurrentUserWriteAccessToCheckList(CheckListDetailsDto checkListDetailsDto);
}
