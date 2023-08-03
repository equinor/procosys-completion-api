using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;

public class CreatePunchItemCommand : IRequest<Result<GuidAndRowVersion>>, IIsProjectCommand
{
    public CreatePunchItemCommand(
        string description,
        Guid projectGuid,
        Guid raisedByOrgGuid,
        Guid clearingByOrgGuid)
    {
        Description = description;
        ProjectGuid = projectGuid;
        RaisedByOrgGuid = raisedByOrgGuid;
        ClearingByOrgGuid = clearingByOrgGuid;
    }

    public string Description { get; }
    public Guid ProjectGuid { get; }
    public Guid RaisedByOrgGuid { get; }
    public Guid ClearingByOrgGuid { get; }
}
