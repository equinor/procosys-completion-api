using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunch;

public class GetPunchQuery : IRequest<Result<PunchDetailsDto>>, IIsPunchQuery
{
    public GetPunchQuery(Guid punchGuid) => PunchGuid = punchGuid;

    public Guid PunchGuid { get; }
}
