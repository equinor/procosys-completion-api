using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemAttachment;

public class UpdatePunchItemAttachmentCommand(
    Guid punchItemGuid,
    Guid attachmentGuid,
    string description,
    IEnumerable<string> labels,
    string rowVersion)
    : CanHaveRestrictionsViaCheckList, IRequest<Result<string>>, IIsPunchItemCommand
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public PunchItem PunchItem { get; set; } = null!;
    public override Guid GetProjectGuidForAccessCheck() => PunchItem.Project.Guid;
    public override Guid GetCheckListGuidForWriteAccessCheck() => PunchItem.CheckListGuid;
    public Guid AttachmentGuid { get; } = attachmentGuid;
    public string Description { get; } = description;
    public IEnumerable<string> Labels { get; } = labels;
    public string RowVersion { get; } = rowVersion;
}
