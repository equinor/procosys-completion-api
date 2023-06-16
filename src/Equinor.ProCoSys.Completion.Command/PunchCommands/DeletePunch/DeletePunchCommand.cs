using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.DeletePunch;

public class DeletePunchCommand : IRequest<Result<Unit>>, IIsPunchCommand
{
    public DeletePunchCommand(Guid punchGuid, string rowVersion)
    {
        PunchGuid = punchGuid;
        RowVersion = rowVersion;
    }

    public Guid PunchGuid { get; }
    public string RowVersion { get; }
}
