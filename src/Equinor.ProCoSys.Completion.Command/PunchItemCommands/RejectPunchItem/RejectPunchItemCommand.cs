using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.RejectPunchItem;

public class RejectPunchItemCommand : IRequest<Result<string>>, IIsPunchItemCommand
{
    public RejectPunchItemCommand(Guid punchItemGuid, string rowVersion)
    {
        PunchItemGuid = punchItemGuid;
        RowVersion = rowVersion;
    }

    public Guid PunchItemGuid { get; }
    public string RowVersion { get; }
}
