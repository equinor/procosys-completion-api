using System;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query;

public interface IIsProjectQuery : IBaseRequest
{
    Guid ProjectGuid { get; }
}
