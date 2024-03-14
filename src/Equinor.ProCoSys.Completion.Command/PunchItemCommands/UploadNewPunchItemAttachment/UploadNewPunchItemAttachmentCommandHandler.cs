using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UploadNewPunchItemAttachment;

public class UploadNewPunchItemAttachmentCommandHandler : IRequestHandler<UploadNewPunchItemAttachmentCommand, Result<GuidAndRowVersion>>
{
    private readonly IAttachmentService _attachmentService;
    private readonly IPunchItemRepository _punchItemRepository;

    public UploadNewPunchItemAttachmentCommandHandler(
        IAttachmentService attachmentService,
        IPunchItemRepository punchItemRepository)
    {
        _attachmentService = attachmentService;
        _punchItemRepository = punchItemRepository;
    }

    public async Task<Result<GuidAndRowVersion>> Handle(UploadNewPunchItemAttachmentCommand request, CancellationToken cancellationToken)
    {
        var project = await _punchItemRepository.GetProjectAsync(request.PunchItemGuid, cancellationToken);

        var attachmentDto = await _attachmentService.UploadNewAsync(
            project.Name,
            nameof(PunchItem),
            request.PunchItemGuid,
            request.FileName,
            request.Content,
            cancellationToken);

        return new SuccessResult<GuidAndRowVersion>(new GuidAndRowVersion(attachmentDto.Guid, attachmentDto.RowVersion));
    }
}
