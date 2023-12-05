using System;
using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemAttachment;

public class UpdatePunchItemAttachmentCommand : IRequest<Result<string>>, IIsPunchItemCommand
{
    public UpdatePunchItemAttachmentCommand(
        Guid punchItemGuid,
        Guid attachmentGuid,
        string description,
        IEnumerable<string> labels,
        string rowVersion)
    {
        PunchItemGuid = punchItemGuid;
        AttachmentGuid = attachmentGuid;
        Description = description;
        Labels = labels;
        RowVersion = rowVersion;
    }

    public Guid PunchItemGuid { get; }
    public Guid AttachmentGuid { get; }
    public string Description { get; }
    public IEnumerable<string> Labels { get; }
    public string RowVersion { get; }
}
