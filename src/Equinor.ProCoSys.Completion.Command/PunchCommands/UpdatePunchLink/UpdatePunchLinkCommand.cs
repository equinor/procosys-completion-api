using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.UpdatePunchLink;

public class UpdatePunchLinkCommand : IRequest<Result<string>>, IIsPunchCommand
{
    public UpdatePunchLinkCommand(Guid punchGuid, Guid linkGuid, string title, string url, string rowVersion)
    {
        PunchGuid = punchGuid;
        LinkGuid = linkGuid;
        Title = title;
        Url = url;
        RowVersion = rowVersion;
    }

    public Guid PunchGuid { get; }
    public Guid LinkGuid { get; }
    public string Title { get; }
    public string Url { get; }
    public string RowVersion { get; }
}
