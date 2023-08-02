using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;

public class CreatePunchItemCommand : IRequest<Result<GuidAndRowVersion>>, IIsProjectCommand
{
    public CreatePunchItemCommand(string description, Guid projectGuid)
    {
        Description = description;
        ProjectGuid = projectGuid;
    }

    public string Description { get; }
    public Guid ProjectGuid { get; }
}
