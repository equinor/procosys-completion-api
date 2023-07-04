using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemLink;

public class UpdatePunchItemLinkCommand : IRequest<Result<string>>, IIsPunchItemCommand
{
    public UpdatePunchItemLinkCommand(Guid punchItemGuid, Guid linkGuid, string title, string url, string rowVersion)
    {
        PunchItemGuid = punchItemGuid;
        LinkGuid = linkGuid;
        Title = title;
        Url = url;
        RowVersion = rowVersion;
    }

    public Guid PunchItemGuid { get; }
    public Guid LinkGuid { get; }
    public string Title { get; }
    public string Url { get; }
    public string RowVersion { get; }
}
