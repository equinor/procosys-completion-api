using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemAttachment;

public class DeletePunchItemAttachmentCommand : IRequest<Result<Unit>>, IIsPunchItemCommand
{
    public DeletePunchItemAttachmentCommand(Guid punchItemGuid, Guid attachmentGuid, string rowVersion)
    {
        PunchItemGuid = punchItemGuid;
        AttachmentGuid = attachmentGuid;
        RowVersion = rowVersion;
    }

    public Guid PunchItemGuid { get; }
    public Guid AttachmentGuid { get; }
    public string RowVersion { get; }
}
