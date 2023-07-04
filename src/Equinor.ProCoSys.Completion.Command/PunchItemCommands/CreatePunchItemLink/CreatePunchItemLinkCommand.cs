using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemLink;

public class CreatePunchItemLinkCommand : IRequest<Result<GuidAndRowVersion>>, IIsPunchItemCommand
{
    public CreatePunchItemLinkCommand(Guid punchItemGuid, string title, string url)
    {
        PunchItemGuid = punchItemGuid;
        Title = title;
        Url = url;
    }

    public Guid PunchItemGuid { get; }
    public string Title { get; }
    public string Url { get; }
}
