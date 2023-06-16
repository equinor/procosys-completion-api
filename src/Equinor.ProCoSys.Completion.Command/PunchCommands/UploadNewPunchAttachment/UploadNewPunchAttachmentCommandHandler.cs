using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.UploadNewPunchAttachment;

public class UploadNewPunchAttachmentCommandHandler : IRequestHandler<UploadNewPunchAttachmentCommand, Result<GuidAndRowVersion>>
{
    private readonly IAttachmentService _attachmentService;

    public UploadNewPunchAttachmentCommandHandler(IAttachmentService attachmentService)
        => _attachmentService = attachmentService;

    public async Task<Result<GuidAndRowVersion>> Handle(UploadNewPunchAttachmentCommand request, CancellationToken cancellationToken)
    {
        var attachmentDto = await _attachmentService.UploadNewAsync(
            nameof(Punch),
            request.PunchGuid,
            request.FileName,
            request.Content,
            cancellationToken);

        return new SuccessResult<GuidAndRowVersion>(new GuidAndRowVersion(attachmentDto.Guid, attachmentDto.RowVersion));
    }
}
