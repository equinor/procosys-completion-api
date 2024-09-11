using System;
using System.IO;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.OverwriteExistingPunchItemAttachment;

public class OverwriteExistingPunchItemAttachmentCommand(
    Guid punchItemGuid,
    string fileName,
    string rowVersion,
    Stream content,
    string contentType)
    : UploadAttachmentCommand(content), ICanHaveRestrictionsViaCheckList, IRequest<string>, IIsPunchItemCommand
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public PunchItem PunchItem { get; set; } = null!;
    public Guid GetProjectGuidForAccessCheck() => PunchItem.Project.Guid;
    public Guid GetCheckListGuidForWriteAccessCheck() => PunchItem.CheckListGuid;
    public CheckListDetailsDto CheckListDetailsDto { get; set; } = null!;
    public string FileName { get; } = fileName;
    public string RowVersion { get; } = rowVersion;
    public string ContentType { get; } = contentType;
}
