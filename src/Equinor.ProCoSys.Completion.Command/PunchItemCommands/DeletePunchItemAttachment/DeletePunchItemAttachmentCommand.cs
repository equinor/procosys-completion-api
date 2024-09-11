using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemAttachment;

public class DeletePunchItemAttachmentCommand(Guid punchItemGuid, Guid attachmentGuid, string rowVersion)
    : ICanHaveRestrictionsViaCheckList, IRequest<Unit>, IIsPunchItemCommand
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public PunchItem PunchItem { get; set; } = null!;
    public Guid GetProjectGuidForAccessCheck() => PunchItem.Project.Guid;
    public Guid GetCheckListGuidForWriteAccessCheck() => PunchItem.CheckListGuid;
    public CheckListDetailsDto CheckListDetailsDto { get; set; } = null!;
    public Guid AttachmentGuid { get; } = attachmentGuid;
    public string RowVersion { get; } = rowVersion;
}
