using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.CreatePunch;

public class CreatePunchCommand : IRequest<Result<GuidAndRowVersion>>, IIsProjectCommand
{
    public CreatePunchCommand(string itemNo, Guid projectGuid)
    {
        ItemNo = itemNo;
        ProjectGuid = projectGuid;
    }

    public string ItemNo { get; }
    public Guid ProjectGuid { get; }
}
