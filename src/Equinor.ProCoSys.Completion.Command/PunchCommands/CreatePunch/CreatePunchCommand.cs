using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.CreatePunch;

public class CreatePunchCommand : IRequest<Result<GuidAndRowVersion>>, IIsProjectCommand
{
    public CreatePunchCommand(string title, Guid projectGuid)
    {
        Title = title;
        ProjectGuid = projectGuid;
    }

    public string Title { get; }
    public Guid ProjectGuid { get; }
}
