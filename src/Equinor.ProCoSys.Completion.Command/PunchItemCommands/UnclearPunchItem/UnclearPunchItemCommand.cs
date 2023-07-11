using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnclearPunchItem;

public class UnclearPunchItemCommand : IRequest<Result<string>>, IIsPunchItemCommand
{
    public UnclearPunchItemCommand(Guid punchItemGuid, string rowVersion)
    {
        PunchItemGuid = punchItemGuid;
        RowVersion = rowVersion;
    }

    public Guid PunchItemGuid { get; }
    public string RowVersion { get; }
}
