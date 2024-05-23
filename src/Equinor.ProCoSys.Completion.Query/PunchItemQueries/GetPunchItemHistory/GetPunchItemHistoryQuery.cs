using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Query.History;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemHistory;

public class GetPunchItemHistoryQuery : IRequest<Result<IEnumerable<HistoryDto>>>, IIsPunchItemQuery
{
    public GetPunchItemHistoryQuery(Guid punchItemGuid) => PunchItemGuid = punchItemGuid;

    public Guid PunchItemGuid { get; }
}
