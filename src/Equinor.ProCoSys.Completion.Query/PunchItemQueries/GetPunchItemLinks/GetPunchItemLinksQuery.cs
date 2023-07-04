using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Query.Links;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemLinks;

public class GetPunchItemLinksQuery : IRequest<Result<IEnumerable<LinkDto>>>, IIsPunchItemQuery
{
    public GetPunchItemLinksQuery(Guid punchItemGuid) => PunchItemGuid = punchItemGuid;

    public Guid PunchItemGuid { get; }
}
