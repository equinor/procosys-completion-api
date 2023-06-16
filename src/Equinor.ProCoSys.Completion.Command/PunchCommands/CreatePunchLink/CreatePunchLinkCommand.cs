using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.CreatePunchLink;

public class CreatePunchLinkCommand : IRequest<Result<GuidAndRowVersion>>, IIsPunchCommand
{
    public CreatePunchLinkCommand(Guid punchGuid, string title, string url)
    {
        PunchGuid = punchGuid;
        Title = title;
        Url = url;
    }

    public Guid PunchGuid { get; }
    public string Title { get; }
    public string Url { get; }
}
