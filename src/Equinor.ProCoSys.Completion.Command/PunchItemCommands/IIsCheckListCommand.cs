using System;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands;

public interface IIsCheckListCommand : IBaseRequest
{
    Guid CheckListGuid { get; }
    CheckListDetailsDto CheckListDetailsDto { get; set; }
}
