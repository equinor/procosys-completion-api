using System;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command;

public interface IIsProjectCommand : IBaseRequest
{
    Guid ProjectGuid { get; }
}
