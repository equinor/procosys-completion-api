using System;
using System.IO;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UploadNewPunchItemAttachment;

public class UploadNewPunchItemAttachmentCommand(
    Guid punchItemGuid,
    string fileName,
    Stream content,
    string contentType)
    : UploadAttachmentCommand(content), IRequest<Result<GuidAndRowVersion>>, IIsPunchItemCommand
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public PunchItem PunchItem { get; set; } = null!;
    public override Guid GetProjectGuidForAccessCheck() => PunchItem.Project.Guid;
    public string FileName { get; } = fileName;
    public string ContentType { get; } = contentType;
}
