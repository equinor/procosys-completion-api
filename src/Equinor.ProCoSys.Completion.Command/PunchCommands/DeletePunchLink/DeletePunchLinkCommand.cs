using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.DeletePunchLink;

public class DeletePunchLinkCommand : IRequest<Result<Unit>>, IIsPunchCommand
{
    public DeletePunchLinkCommand(Guid punchGuid, Guid linkGuid, string rowVersion)
    {
        PunchGuid = punchGuid;
        LinkGuid = linkGuid;
        RowVersion = rowVersion;
    }

    public Guid PunchGuid { get; }
    public Guid LinkGuid { get; }
    public string RowVersion { get; }
}
