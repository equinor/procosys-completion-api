using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Query.Links;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchLinks;

public class GetPunchLinksQuery : IRequest<Result<IEnumerable<LinkDto>>>, IIsPunchQuery
{
    public GetPunchLinksQuery(Guid punchGuid) => PunchGuid = punchGuid;

    public Guid PunchGuid { get; }
}
