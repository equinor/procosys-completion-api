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

    public UploadNewPunchItemAttachmentCommandHandler(IAttachmentService attachmentService)
        => _attachmentService = attachmentService;

    public async Task<Result<GuidAndRowVersion>> Handle(UploadNewPunchItemAttachmentCommand request, CancellationToken cancellationToken)
    {
        var attachmentDto = await _attachmentService.UploadNewAsync(
            nameof(PunchItem),
            request.PunchItemGuid,
            request.FileName,
            request.Description,
            request.Content,
            cancellationToken);

        return new SuccessResult<GuidAndRowVersion>(new GuidAndRowVersion(attachmentDto.Guid, attachmentDto.RowVersion));
    }
}
