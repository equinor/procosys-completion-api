using System;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command;

public interface IIsPunchCommand : IBaseRequest
{
    Guid PunchGuid { get; }
}
