using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;

public class DeletePunchItemCommand : IRequest<Result<Unit>>, IIsPunchItemCommand
{
    public DeletePunchItemCommand(Guid punchItemGuid, string rowVersion)
    {
        PunchItemGuid = punchItemGuid;
        RowVersion = rowVersion;
    }

    public Guid PunchItemGuid { get; }
    public string RowVersion { get; }
}
