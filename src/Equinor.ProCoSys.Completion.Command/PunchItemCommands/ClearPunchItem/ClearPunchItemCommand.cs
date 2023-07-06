using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.ClearPunchItem;

public class ClearPunchItemCommand : IRequest<Result<string>>, IIsPunchItemCommand
{
    public ClearPunchItemCommand(Guid punchItemGuid, string rowVersion)
    {
        PunchItemGuid = punchItemGuid;
        RowVersion = rowVersion;
    }

    public Guid PunchItemGuid { get; }
    public string RowVersion { get; }
}
