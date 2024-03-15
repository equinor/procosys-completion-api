using System;
using System.IO;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.OverwriteExistingPunchItemAttachment;

public class OverwriteExistingPunchItemAttachmentCommand : UploadAttachmentCommand, IRequest<Result<string>>, IIsPunchItemCommand
{
    public OverwriteExistingPunchItemAttachmentCommand(Guid punchItemGuid, string fileName, string rowVersion, Stream content, string contentType)
    : base(content)
    {
        PunchItemGuid = punchItemGuid;
        FileName = fileName;
        RowVersion = rowVersion;
        ContentType = contentType;
    }

    public Guid PunchItemGuid { get; }

    public string FileName { get; }
    public string RowVersion { get; }
    public string ContentType { get; }
}
