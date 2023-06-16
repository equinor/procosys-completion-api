using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.UpdatePunch;

public class UpdatePunchCommand : IRequest<Result<string>>, IIsPunchCommand
{
    public UpdatePunchCommand(Guid punchGuid, string title, string? text, string rowVersion)
    {
        PunchGuid = punchGuid;
        Title = title;
        Text = text;
        RowVersion = rowVersion;
    }

    public Guid PunchGuid { get; }
    public string Title { get; }
    public string? Text { get; }
    public string RowVersion { get; }
}
