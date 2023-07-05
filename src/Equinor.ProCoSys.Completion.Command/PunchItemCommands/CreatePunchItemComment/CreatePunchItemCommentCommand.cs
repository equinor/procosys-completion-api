using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemComment;

public class CreatePunchItemCommentCommand : IRequest<Result<GuidAndRowVersion>>, IIsPunchItemCommand
{
    public CreatePunchItemCommentCommand(Guid punchItemGuid, string text)
    {
        PunchItemGuid = punchItemGuid;
        Text = text;
    }

    public Guid PunchItemGuid { get; }
    public string Text { get; }
}
