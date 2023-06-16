using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.Comments;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchComments;

public class GetPunchCommentsQueryHandler : IRequestHandler<GetPunchCommentsQuery, Result<IEnumerable<CommentDto>>>
{
    private readonly ICommentService _commentService;

    public GetPunchCommentsQueryHandler(ICommentService commentService) => _commentService = commentService;

    public async Task<Result<IEnumerable<CommentDto>>> Handle(GetPunchCommentsQuery request, CancellationToken cancellationToken)
    {
        var commentDtos = await _commentService.GetAllForSourceAsync(request.PunchGuid, cancellationToken);
        return new SuccessResult<IEnumerable<CommentDto>>(commentDtos);
    }
}
