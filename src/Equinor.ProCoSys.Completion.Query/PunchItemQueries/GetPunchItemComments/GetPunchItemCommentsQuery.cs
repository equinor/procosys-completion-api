using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Query.Comments;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemComments;

public class GetPunchItemCommentsQuery : IRequest<Result<IEnumerable<CommentDto>>>, IIsPunchItemQuery
{
    public GetPunchItemCommentsQuery(Guid punchItemGuid) => PunchItemGuid = punchItemGuid;

    public Guid PunchItemGuid { get; }
}
