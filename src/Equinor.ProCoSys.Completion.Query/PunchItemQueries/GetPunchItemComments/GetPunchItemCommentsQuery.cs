using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Query.Comments;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemComments;

public class GetPunchItemCommentsQuery(Guid punchItemGuid)
    : INeedProjectAccess, IRequest<Result<IEnumerable<CommentDto>>>, IIsPunchItemRelatedQuery
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public ProjectDetailsDto ProjectDetailsDto { get; set; } = null!;
    public Guid GetProjectGuidForAccessCheck() => ProjectDetailsDto.Guid;
}
