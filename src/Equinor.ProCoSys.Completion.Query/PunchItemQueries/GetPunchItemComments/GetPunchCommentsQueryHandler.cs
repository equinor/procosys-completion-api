using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.Comments;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemComments;

public class GetPunchItemCommentsQueryHandler : IRequestHandler<GetPunchItemCommentsQuery, Result<IEnumerable<CommentDto>>>
{
    private readonly ICommentService _commentService;

    public GetPunchItemCommentsQueryHandler(ICommentService commentService) => _commentService = commentService;

    public async Task<Result<IEnumerable<CommentDto>>> Handle(GetPunchItemCommentsQuery request, CancellationToken cancellationToken)
    {
        var commentDtos = await _commentService.GetAllForSourceAsync(request.PunchItemGuid, cancellationToken);
        return new SuccessResult<IEnumerable<CommentDto>>(commentDtos);
    }
}
