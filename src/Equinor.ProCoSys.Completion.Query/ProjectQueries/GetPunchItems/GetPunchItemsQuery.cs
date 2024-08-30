using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.ProjectQueries.GetPunchItems;

public class GetPunchItemsQuery(Guid projectGuid)
    : INeedProjectAccess, IRequest<Result<IEnumerable<PunchItemDto>>>
{
    public Guid ProjectGuid { get; } = projectGuid;
    public Guid GetProjectGuidForAccessCheck() => ProjectGuid;
}
