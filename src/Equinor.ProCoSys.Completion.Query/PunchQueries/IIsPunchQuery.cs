using System;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries;

public interface IIsPunchQuery : IBaseRequest
{
    Guid PunchGuid { get; }
}
