using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemLink;

public class DeletePunchItemLinkCommand : IRequest<Result<Unit>>, IIsPunchItemCommand
{
    public DeletePunchItemLinkCommand(Guid punchItemGuid, Guid linkGuid, string rowVersion)
    {
        PunchItemGuid = punchItemGuid;
        LinkGuid = linkGuid;
        RowVersion = rowVersion;
    }

    public Guid PunchItemGuid { get; }
    public Guid LinkGuid { get; }
    public string RowVersion { get; }
}
