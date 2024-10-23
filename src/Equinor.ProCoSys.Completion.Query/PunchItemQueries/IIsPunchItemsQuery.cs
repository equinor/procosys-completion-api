using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries;

public interface IIsPunchItemsQuery : IBaseRequest
{
    IEnumerable<Guid> PunchItemGuids { get; }
    IEnumerable<PunchItemTinyDetailsDto> PunchItemsDetailsDto { get; set; }
}
