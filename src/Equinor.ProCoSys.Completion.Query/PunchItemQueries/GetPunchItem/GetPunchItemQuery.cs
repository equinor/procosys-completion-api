using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItem;

public class GetPunchItemQuery : IRequest<Result<PunchItemDetailsDto>>, IIsPunchItemQuery
{
    public GetPunchItemQuery(Guid punchItemGuid) => PunchItemGuid = punchItemGuid;

    public Guid PunchItemGuid { get; }
}
