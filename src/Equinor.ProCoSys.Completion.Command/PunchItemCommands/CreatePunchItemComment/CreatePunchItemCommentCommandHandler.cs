using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Comments;
using MediatR;
using ServiceResult;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemComment;

public class CreatePunchItemCommentCommandHandler : IRequestHandler<CreatePunchItemCommentCommand, Result<GuidAndRowVersion>>
{
    private readonly ICommentService _commentService;
    private readonly ILabelRepository _labelRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePunchItemCommentCommandHandler(
        ICommentService commentService,
        ILabelRepository labelRepository,
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork)
    {
        _commentService = commentService;
        _labelRepository = labelRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<GuidAndRowVersion>> Handle(
        CreatePunchItemCommentCommand request,
        CancellationToken cancellationToken)
    {
        var labels = await _labelRepository.GetManyAsync(request.Labels, cancellationToken);
        var mentions = await _personRepository.GetOrCreateManyAsync(request.Mentions, cancellationToken);

        var commentDto = await _commentService.AddAndSaveAsync(
            _unitOfWork,
            nameof(PunchItem),
            request.PunchItemGuid,
            request.Text,
            labels,
            mentions,
            cancellationToken);

        return new SuccessResult<GuidAndRowVersion>(new GuidAndRowVersion(commentDto.Guid, commentDto.RowVersion));
    }
}
