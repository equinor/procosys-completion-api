using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UploadNewPunchItemAttachment;

public class UploadNewPunchItemAttachmentCommandHandler(IAttachmentService attachmentService)
    : IRequestHandler<UploadNewPunchItemAttachmentCommand, GuidAndRowVersion>
{
    public async Task<GuidAndRowVersion> Handle(UploadNewPunchItemAttachmentCommand request, CancellationToken cancellationToken)
    {
        var punchItem = request.PunchItem;
        var attachmentDto = await attachmentService.UploadNewAsync(
            punchItem.Project.Name,
            nameof(PunchItem),
            request.PunchItemGuid,
            request.FileName,
            request.Content,
            request.ContentType,
            cancellationToken);

        return new GuidAndRowVersion(attachmentDto.Guid, attachmentDto.RowVersion);
    }
}
