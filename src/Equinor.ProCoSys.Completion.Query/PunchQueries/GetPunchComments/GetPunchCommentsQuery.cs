using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Query.Comments;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchComments;

public class GetPunchCommentsQuery : IRequest<Result<IEnumerable<CommentDto>>>, IIsPunchQuery
{
    public GetPunchCommentsQuery(Guid punchGuid) => PunchGuid = punchGuid;

    public Guid PunchGuid { get; }
}
