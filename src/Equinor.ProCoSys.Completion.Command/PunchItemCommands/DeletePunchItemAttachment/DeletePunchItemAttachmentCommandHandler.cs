using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemAttachment;

public class DeletePunchItemAttachmentCommandHandler : IRequestHandler<DeletePunchItemAttachmentCommand, Unit>
{
    private readonly IAttachmentService _attachmentService;

    public DeletePunchItemAttachmentCommandHandler(IAttachmentService attachmentService) => _attachmentService = attachmentService;

    public async Task<Unit> Handle(DeletePunchItemAttachmentCommand request, CancellationToken cancellationToken)
    {
        await _attachmentService.DeleteAsync(
            request.AttachmentGuid,
            request.RowVersion,
            cancellationToken);

        return Unit.Value;
    }
}
