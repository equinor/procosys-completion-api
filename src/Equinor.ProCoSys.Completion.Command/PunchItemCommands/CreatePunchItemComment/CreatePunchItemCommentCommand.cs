using System;
using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemComment;

public class CreatePunchItemCommentCommand : IRequest<Result<GuidAndRowVersion>>, IIsPunchItemCommand
{
    public CreatePunchItemCommentCommand(
        Guid punchItemGuid,
        string text,
        IEnumerable<string> labels,
        IEnumerable<Guid> mentions)
    {
        PunchItemGuid = punchItemGuid;
        Text = text;
        Labels = labels;
        Mentions = mentions;
    }

    public Guid PunchItemGuid { get; }
    public string Text { get; }
    public IEnumerable<string> Labels { get; }
    public IEnumerable<Guid> Mentions { get; }
}
