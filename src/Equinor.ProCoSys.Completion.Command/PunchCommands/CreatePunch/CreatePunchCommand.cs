using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.CreatePunch;

public class CreatePunchCommand : IRequest<Result<GuidAndRowVersion>>, IIsProjectCommand
{
    public CreatePunchCommand(string title, string projectName)
    {
        Title = title;
        ProjectName = projectName;
    }

    public string Title { get; }
    public string ProjectName { get; }
}
