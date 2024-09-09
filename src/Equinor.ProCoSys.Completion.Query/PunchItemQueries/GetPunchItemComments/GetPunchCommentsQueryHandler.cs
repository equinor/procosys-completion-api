using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.Comments;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemComments;

public class GetPunchItemCommentsQueryHandler : IRequestHandler<GetPunchItemCommentsQuery, IEnumerable<CommentDto>>
{
    private readonly ICommentService _commentService;

    public GetPunchItemCommentsQueryHandler(ICommentService commentService) => _commentService = commentService;

    public async Task<IEnumerable<CommentDto>> Handle(GetPunchItemCommentsQuery request, CancellationToken cancellationToken)
    {
        var commentDtos = await _commentService.GetAllForParentAsync(request.PunchItemGuid, cancellationToken);
        return commentDtos;
    }
}
