using System;
using System.IO;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.OverwriteExistingPunchItemAttachment;

public class OverwriteExistingPunchItemAttachmentCommand(
    Guid punchItemGuid,
    string fileName,
    string rowVersion,
    Stream content,
    string contentType)
    : UploadAttachmentCommand(content), IRequest<Result<string>>, IIsPunchItemCommand
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public PunchItem PunchItem { get; set; } = null!;

    public string FileName { get; } = fileName;
    public string RowVersion { get; } = rowVersion;
    public string ContentType { get; } = contentType;
}
