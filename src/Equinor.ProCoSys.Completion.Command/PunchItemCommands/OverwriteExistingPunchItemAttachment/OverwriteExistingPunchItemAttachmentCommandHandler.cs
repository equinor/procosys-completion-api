using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.OverwriteExistingPunchItemAttachment;

public class OverwriteExistingPunchItemAttachmentCommandHandler : IRequestHandler<OverwriteExistingPunchItemAttachmentCommand, string>
{
    private readonly IAttachmentService _attachmentService;

    public OverwriteExistingPunchItemAttachmentCommandHandler(IAttachmentService attachmentService)
        => _attachmentService = attachmentService;

    public async Task<string> Handle(OverwriteExistingPunchItemAttachmentCommand request, CancellationToken cancellationToken)
    {
        var newRowVersion = await _attachmentService.UploadOverwriteAsync(
            nameof(PunchItem),
            request.PunchItemGuid,
            request.FileName,
            request.Content,
            request.ContentType,
            request.RowVersion,
            cancellationToken);

        return newRowVersion;
    }
}
