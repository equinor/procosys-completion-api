using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsInProject;

public class GetPunchItemsInProjectQuery(Guid projectGuid)
    : NeedProjectAccess, IRequest<Result<IEnumerable<PunchItemDto>>>
{
    public Guid ProjectGuid { get; } = projectGuid;
    public override Guid GetProjectGuidForAccessCheck() => ProjectGuid;
}
