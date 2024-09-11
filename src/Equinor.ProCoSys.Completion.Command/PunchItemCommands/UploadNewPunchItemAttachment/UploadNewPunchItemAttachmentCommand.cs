using System;
using System.IO;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UploadNewPunchItemAttachment;

public class UploadNewPunchItemAttachmentCommand(
    Guid punchItemGuid,
    string fileName,
    Stream content,
    string contentType)
    : UploadAttachmentCommand(content), ICanHaveRestrictionsViaCheckList, IRequest<GuidAndRowVersion>, IIsPunchItemCommand
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public PunchItem PunchItem { get; set; } = null!;
    public Guid GetProjectGuidForAccessCheck() => PunchItem.Project.Guid;
    public Guid GetCheckListGuidForWriteAccessCheck() => PunchItem.CheckListGuid;
    public CheckListDetailsDto CheckListDetailsDto { get; set; } = null!;
    public string FileName { get; } = fileName;
    public string ContentType { get; } = contentType;
}
