using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.VerifyPunchItem;

public class VerifyPunchItemCommand : IRequest<Result<string>>, IIsPunchItemCommand
{
    public VerifyPunchItemCommand(Guid punchItemGuid, string rowVersion)
    {
        PunchItemGuid = punchItemGuid;
        RowVersion = rowVersion;
    }

    public Guid PunchItemGuid { get; }
    public string RowVersion { get; }
}
