using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemAttachment;

public class DeletePunchItemAttachmentCommand(Guid punchItemGuid, Guid attachmentGuid, string rowVersion)
    : IRequest<Result<Unit>>, IIsPunchItemCommand
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public PunchItem PunchItem { get; set; } = null!;
    public Guid AttachmentGuid { get; } = attachmentGuid;
    public string RowVersion { get; } = rowVersion;
}
