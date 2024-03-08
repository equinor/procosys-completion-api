using System;
using System.IO;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UploadNewPunchItemAttachment;

public class UploadNewPunchItemAttachmentCommand : UploadAttachmentCommand, IRequest<Result<GuidAndRowVersion>>, IIsPunchItemCommand
{
    public UploadNewPunchItemAttachmentCommand(Guid punchItemGuid, string fileName, Stream content)
        : base(content)
    {
        PunchItemGuid = punchItemGuid;
        FileName = fileName;
    }

    public UploadNewPunchItemAttachmentCommand(Guid punchItemGuid, string fileName, string? description, Stream content)
    : base(content)
    {
        PunchItemGuid = punchItemGuid;
        FileName = fileName;
        Description = description;
    }

    public Guid PunchItemGuid { get; }

    public string FileName { get; }

    public string? Description { get; }
}
