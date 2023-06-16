using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.VoidPunch;

public class VoidPunchCommand : IRequest<Result<string>>, IIsPunchCommand
{
    public VoidPunchCommand(Guid punchGuid, string rowVersion)
    {
        PunchGuid = punchGuid;
        RowVersion = rowVersion;
    }

    public Guid PunchGuid { get; }
    public string RowVersion { get; }
}
