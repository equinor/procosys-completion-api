using System;
using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsInProject;

public class GetPunchItemsInProjectQuery : IRequest<Result<IEnumerable<PunchItemDto>>>, IIsProjectQuery
{
    public GetPunchItemsInProjectQuery(Guid projectGuid) => ProjectGuid = projectGuid;

    public Guid ProjectGuid { get; }
}
