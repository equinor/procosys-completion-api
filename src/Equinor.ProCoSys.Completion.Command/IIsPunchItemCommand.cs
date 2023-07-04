using System;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command;

public interface IIsPunchItemCommand : IBaseRequest
{
    Guid PunchItemGuid { get; }
}
