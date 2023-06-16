using System;
using System.IO;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.UploadNewPunchAttachment;

public class UploadNewPunchAttachmentCommand : UploadAttachmentCommand, IRequest<Result<GuidAndRowVersion>>, IIsPunchCommand
{
    public UploadNewPunchAttachmentCommand(Guid punchGuid, string fileName, Stream content)
        : base(content)
    {
        PunchGuid = punchGuid;
        FileName = fileName;
    }

    public Guid PunchGuid { get; }

    public string FileName { get; }
}
