using System;
using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.RejectPunchItem;

public class RejectPunchItemCommand : IRequest<Result<string>>, IIsPunchItemCommand
{
    public RejectPunchItemCommand(
        Guid punchItemGuid,
        string comment,
        IEnumerable<Guid> mentions,
        string rowVersion)
    {
        PunchItemGuid = punchItemGuid;
        Comment = comment;
        Mentions = mentions;
        RowVersion = rowVersion;
    }

    public Guid PunchItemGuid { get; }
    public string Comment { get; }
    public IEnumerable<Guid> Mentions { get; }
    public string RowVersion { get; }
}
