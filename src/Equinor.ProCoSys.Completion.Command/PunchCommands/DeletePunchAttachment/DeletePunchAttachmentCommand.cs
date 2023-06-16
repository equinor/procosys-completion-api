using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.DeletePunchAttachment;

public class DeletePunchAttachmentCommand : IRequest<Result<Unit>>, IIsPunchCommand
{
    public DeletePunchAttachmentCommand(Guid punchGuid, Guid attachmentGuid, string rowVersion)
    {
        PunchGuid = punchGuid;
        AttachmentGuid = attachmentGuid;
        RowVersion = rowVersion;
    }

    public Guid PunchGuid { get; }
    public Guid AttachmentGuid { get; }
    public string RowVersion { get; }
}
