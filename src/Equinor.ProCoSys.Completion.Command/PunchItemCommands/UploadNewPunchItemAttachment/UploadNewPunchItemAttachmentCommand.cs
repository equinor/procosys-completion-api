using System;
using System.IO;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UploadNewPunchItemAttachment;

public class UploadNewPunchItemAttachmentCommand : UploadAttachmentCommand, IRequest<Result<GuidAndRowVersion>>, IIsPunchItemCommand
{
    public UploadNewPunchItemAttachmentCommand(Guid punchItemGuid, string fileName, Stream content, string contentType)
    : base(content)
    {
        PunchItemGuid = punchItemGuid;
        FileName = fileName;
        ContentType = contentType;
    }

    public Guid PunchItemGuid { get; }

    public string FileName { get; }
    public string ContentType { get; }
}
