using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.UpdatePunch;

public class UpdatePunchCommand : IRequest<Result<string>>, IIsPunchCommand
{
    public UpdatePunchCommand(Guid punchGuid, string? description, string rowVersion)
    {
        PunchGuid = punchGuid;
        Description = description;
        RowVersion = rowVersion;
    }

    public Guid PunchGuid { get; }
    public string? Description { get; }
    public string RowVersion { get; }
}
