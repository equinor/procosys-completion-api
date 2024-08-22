using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemComment;

public class CreatePunchItemCommentCommand(
    Guid punchItemGuid,
    string text,
    IEnumerable<string> labels,
    IEnumerable<Guid> mentions)
    : ICanHaveRestrictionsViaCheckList, IRequest<Result<GuidAndRowVersion>>, IIsPunchItemCommand
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public PunchItem PunchItem { get; set; } = null!;
    public Guid GetProjectGuidForAccessCheck() => PunchItem.Project.Guid;
    public Guid GetCheckListGuidForWriteAccessCheck() => PunchItem.CheckListGuid;
    public CheckListDetailsDto CheckListDetailsDto { get; set; } = null!;

    public string Text { get; } = text;
    public IEnumerable<string> Labels { get; } = labels;
    public IEnumerable<Guid> Mentions { get; } = mentions;
}
