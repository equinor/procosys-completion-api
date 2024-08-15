using System;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries;

public interface IIsPunchItemQuery : IBaseRequest
{
    Guid PunchItemGuid { get; }
    PunchItemDetailsDto PunchItemDetailsDto { get; set; }
}
