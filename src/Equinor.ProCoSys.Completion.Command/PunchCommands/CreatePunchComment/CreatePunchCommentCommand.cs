using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.CreatePunchComment;

public class CreatePunchCommentCommand : IRequest<Result<GuidAndRowVersion>>, IIsPunchCommand
{
    public CreatePunchCommentCommand(Guid punchGuid, string text)
    {
        PunchGuid = punchGuid;
        Text = text;
    }

    public Guid PunchGuid { get; }
    public string Text { get; }
}
