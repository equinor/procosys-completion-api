using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;

public class CreatePunchItemCommand : IRequest<Result<GuidAndRowVersion>>, IIsProjectCommand
{
    public CreatePunchItemCommand(
        string description,
        Guid projectGuid,
        Guid checkListGuid,
        Guid raisedByOrgGuid,
        Guid clearingByOrgGuid,
        Guid? priorityGuid = null,
        Guid? sortingGuid = null,
        Guid? typeGuid = null)
    {
        Description = description;
        ProjectGuid = projectGuid;
        CheckListGuid = checkListGuid;
        RaisedByOrgGuid = raisedByOrgGuid;
        ClearingByOrgGuid = clearingByOrgGuid;
        PriorityGuid = priorityGuid;
        SortingGuid = sortingGuid;
        TypeGuid = typeGuid;
    }

    public string Description { get; }
    public Guid ProjectGuid { get; }
    public Guid CheckListGuid { get; }
    public Guid RaisedByOrgGuid { get; }
    public Guid ClearingByOrgGuid { get; }
    public Guid? PriorityGuid { get; }
    public Guid? SortingGuid { get; }
    public Guid? TypeGuid { get; }
}
