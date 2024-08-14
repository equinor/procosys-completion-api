using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemLink;

public class DeletePunchItemLinkCommand(Guid punchItemGuid, Guid linkGuid, string rowVersion)
    : IRequest<Result<Unit>>, IIsPunchItemCommand
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public PunchItem PunchItem { get; set; } = null!;
    public Guid LinkGuid { get; } = linkGuid;
    public string RowVersion { get; } = rowVersion;
}
