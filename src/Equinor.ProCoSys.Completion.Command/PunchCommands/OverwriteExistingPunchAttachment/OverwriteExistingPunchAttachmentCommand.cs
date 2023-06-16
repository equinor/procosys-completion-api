using System;
using System.IO;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.OverwriteExistingPunchAttachment;

public class OverwriteExistingPunchAttachmentCommand : UploadAttachmentCommand, IRequest<Result<string>>, IIsPunchCommand
{
    public OverwriteExistingPunchAttachmentCommand(Guid punchGuid, string fileName, string rowVersion, Stream content)
        : base(content)
    {
        PunchGuid = punchGuid;
        FileName = fileName;
        RowVersion = rowVersion;
    }

    public Guid PunchGuid { get; }

    public string FileName { get; }
    public string RowVersion { get; }
}
