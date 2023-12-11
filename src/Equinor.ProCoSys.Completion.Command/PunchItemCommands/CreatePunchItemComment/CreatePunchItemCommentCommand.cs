using System;
using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemComment;

public class CreatePunchItemCommentCommand : IRequest<Result<GuidAndRowVersion>>, IIsPunchItemCommand
{
    public CreatePunchItemCommentCommand(Guid punchItemGuid, string text, IEnumerable<string> labels)
    {
        PunchItemGuid = punchItemGuid;
        Text = text;
        Labels = labels;
    }

    public Guid PunchItemGuid { get; }
    public string Text { get; }
    public IEnumerable<string> Labels { get; }
}
