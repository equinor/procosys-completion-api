using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.DeletePunchAttachment;

public class DeletePunchAttachmentCommandHandler : IRequestHandler<DeletePunchAttachmentCommand, Result<Unit>>
{
    private readonly IAttachmentService _attachmentService;

    public DeletePunchAttachmentCommandHandler(IAttachmentService attachmentService) => _attachmentService = attachmentService;

    public async Task<Result<Unit>> Handle(DeletePunchAttachmentCommand request, CancellationToken cancellationToken)
    {
        await _attachmentService.DeleteAsync(
            request.AttachmentGuid,
            request.RowVersion,
            cancellationToken);

        return new SuccessResult<Unit>(Unit.Value);
    }
}
