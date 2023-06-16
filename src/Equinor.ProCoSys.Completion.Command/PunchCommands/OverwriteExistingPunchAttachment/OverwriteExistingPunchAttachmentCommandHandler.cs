using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.OverwriteExistingPunchAttachment;

public class OverwriteExistingPunchAttachmentCommandHandler : IRequestHandler<OverwriteExistingPunchAttachmentCommand, Result<string>>
{
    private readonly IAttachmentService _attachmentService;

    public OverwriteExistingPunchAttachmentCommandHandler(IAttachmentService attachmentService)
        => _attachmentService = attachmentService;

    public async Task<Result<string>> Handle(OverwriteExistingPunchAttachmentCommand request, CancellationToken cancellationToken)
    {
        var newRowVersion = await _attachmentService.UploadOverwriteAsync(
            nameof(Punch),
            request.PunchGuid,
            request.FileName,
            request.Content,
            request.RowVersion,
            cancellationToken);

        return new SuccessResult<string>(newRowVersion);
    }
}
