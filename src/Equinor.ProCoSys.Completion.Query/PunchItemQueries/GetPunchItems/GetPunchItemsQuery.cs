using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItems;

public class GetPunchItemsQuery(IEnumerable<Guid> punchItemGuids)
    : INeedProjectsAccess, IRequest<IEnumerable<PunchItemTinyDetailsDto>>, IIsPunchItemsQuery
{
    public IEnumerable<Guid> PunchItemGuids { get; } = punchItemGuids;
    public IEnumerable<PunchItemTinyDetailsDto> PunchItemsDetailsDto { get; set; } = null!;
    public IEnumerable<Guid> GetProjectGuidsForAccessCheck() 
        => PunchItemsDetailsDto.Select(dto => dto.ProjectGuid).Distinct();
}
