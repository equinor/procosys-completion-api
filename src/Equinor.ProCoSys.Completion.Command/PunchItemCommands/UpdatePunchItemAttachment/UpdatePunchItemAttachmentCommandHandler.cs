using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemAttachment;

public class UpdatePunchItemAttachmentCommandHandler : IRequestHandler<UpdatePunchItemAttachmentCommand, Result<string>>
{
    private readonly IAttachmentService _attachmentService;
    private readonly ILabelRepository _labelRepository;

    public UpdatePunchItemAttachmentCommandHandler(IAttachmentService attachmentService, ILabelRepository labelRepository)
    {
        _attachmentService = attachmentService;
        _labelRepository = labelRepository;
    }

    public async Task<Result<string>> Handle(UpdatePunchItemAttachmentCommand request, CancellationToken cancellationToken)
    {
        var labels = await _labelRepository.GetManyAsync(request.Labels, cancellationToken);

        var newRowVersion = await _attachmentService.UpdateAsync(
            request.AttachmentGuid,
            request.Description,
            labels,
            request.RowVersion,
            cancellationToken);

        return new SuccessResult<string>(newRowVersion);
    }
}
