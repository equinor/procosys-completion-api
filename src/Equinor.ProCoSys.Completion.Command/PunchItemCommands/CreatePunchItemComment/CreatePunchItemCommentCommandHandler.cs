using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Comments;
using MediatR;
using ServiceResult;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemComment;

public class CreatePunchItemCommentCommandHandler : IRequestHandler<CreatePunchItemCommentCommand, Result<GuidAndRowVersion>>
{
    private readonly ICommentService _commentService;
    private readonly ILabelRepository _labelRepository;

    public CreatePunchItemCommentCommandHandler(ICommentService commentService, ILabelRepository labelRepository)
    {
        _commentService = commentService;
        _labelRepository = labelRepository;
    }

    public async Task<Result<GuidAndRowVersion>> Handle(
        CreatePunchItemCommentCommand request,
        CancellationToken cancellationToken)
    {
        var labels = await _labelRepository.GetManyAsync(request.Labels, cancellationToken);

        var commentDto = await _commentService.AddAsync(
            nameof(PunchItem),
            request.PunchItemGuid,
            request.Text,
            labels,
            cancellationToken);

        return new SuccessResult<GuidAndRowVersion>(new GuidAndRowVersion(commentDto.Guid, commentDto.RowVersion));
    }
}
