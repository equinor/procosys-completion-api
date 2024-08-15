using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Query.History;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemHistory;

public class GetPunchItemHistoryQuery(Guid punchItemGuid)
    : NeedProjectAccess, IRequest<Result<IEnumerable<HistoryDto>>>, IIsPunchItemRelatedQuery
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public ProjectDetailsDto ProjectDetailsDto { get; set; } = null!;
    public override Guid GetProjectGuidForAccessCheck() => ProjectDetailsDto.Guid;
}
