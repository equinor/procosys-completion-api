using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.RejectPunchItem;

public class RejectPunchItemCommand : IRequest<Result<string>>, IIsPunchItemCommand
{
    public RejectPunchItemCommand(Guid punchItemGuid, string comment, string rowVersion)
    {
        PunchItemGuid = punchItemGuid;
        Comment = comment;
        RowVersion = rowVersion;
    }

    public Guid PunchItemGuid { get; }
    public string Comment { get; }
    public string RowVersion { get; }
}
