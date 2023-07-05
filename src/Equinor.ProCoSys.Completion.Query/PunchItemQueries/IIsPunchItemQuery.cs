using System;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries;

public interface IIsPunchItemQuery : IBaseRequest
{
    Guid PunchItemGuid { get; }
}
