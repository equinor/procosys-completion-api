using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Comments;
using MediatR;
using ServiceResult;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.CreatePunchComment;

public class CreatePunchCommentCommandHandler : IRequestHandler<CreatePunchCommentCommand, Result<GuidAndRowVersion>>
{
    private readonly ICommentService _commentService;

    public CreatePunchCommentCommandHandler(ICommentService commentService) => _commentService = commentService;

    public async Task<Result<GuidAndRowVersion>> Handle(CreatePunchCommentCommand request, CancellationToken cancellationToken)
    {
        var commentDto = await _commentService.AddAsync(nameof(Punch), request.PunchGuid, request.Text, cancellationToken);

        return new SuccessResult<GuidAndRowVersion>(new GuidAndRowVersion(commentDto.Guid, commentDto.RowVersion));
    }
}
