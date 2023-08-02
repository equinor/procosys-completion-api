using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;

public class UpdatePunchItemCommand : IRequest<Result<string>>, IIsPunchItemCommand
{
    public UpdatePunchItemCommand(Guid punchItemGuid, string description, string rowVersion)
    {
        PunchItemGuid = punchItemGuid;
        Description = description;
        RowVersion = rowVersion;
    }

    public Guid PunchItemGuid { get; }
    public string Description { get; }
    public string RowVersion { get; }
}
