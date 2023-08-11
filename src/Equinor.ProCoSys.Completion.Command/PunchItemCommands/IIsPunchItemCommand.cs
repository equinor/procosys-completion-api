using System;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands;

public interface IIsPunchItemCommand : IBaseRequest
{
    Guid PunchItemGuid { get; }
}
