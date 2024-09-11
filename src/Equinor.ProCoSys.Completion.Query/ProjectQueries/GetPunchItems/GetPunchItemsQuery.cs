using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.ProjectQueries.GetPunchItems;

public class GetPunchItemsQuery(Guid projectGuid)
    : INeedProjectAccess, IRequest<IEnumerable<PunchItemDto>>
{
    public Guid ProjectGuid { get; } = projectGuid;
    public Guid GetProjectGuidForAccessCheck() => ProjectGuid;
}
