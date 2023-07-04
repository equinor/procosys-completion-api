using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;

public class CreatePunchItemCommand : IRequest<Result<GuidAndRowVersion>>, IIsProjectCommand
{
    public CreatePunchItemCommand(string itemNo, Guid projectGuid)
    {
        ItemNo = itemNo;
        ProjectGuid = projectGuid;
    }

    public string ItemNo { get; }
    public Guid ProjectGuid { get; }
}
