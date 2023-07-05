using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Comments;
using MediatR;
using ServiceResult;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemComment;

public class CreatePunchItemCommentCommandHandler : IRequestHandler<CreatePunchItemCommentCommand, Result<GuidAndRowVersion>>
{
    private readonly ICommentService _commentService;

    public CreatePunchItemCommentCommandHandler(ICommentService commentService) => _commentService = commentService;

    public async Task<Result<GuidAndRowVersion>> Handle(CreatePunchItemCommentCommand request, CancellationToken cancellationToken)
    {
        var commentDto = await _commentService.AddAsync(nameof(PunchItem), request.PunchItemGuid, request.Text, cancellationToken);

        return new SuccessResult<GuidAndRowVersion>(new GuidAndRowVersion(commentDto.Guid, commentDto.RowVersion));
    }
}
