using System;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries;

public interface IIsPunchItemRelatedQuery : IBaseRequest
{
    Guid PunchItemGuid { get; }
    ProjectDetailsDto ProjectDetailsDto { get; set; }
}
