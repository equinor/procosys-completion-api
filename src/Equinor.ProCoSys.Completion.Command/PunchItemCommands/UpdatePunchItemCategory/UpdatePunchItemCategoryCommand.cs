using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemCategory;

public class UpdatePunchItemCategoryCommand(Guid punchItemGuid, Category category, string rowVersion)
    : ICanHaveRestrictionsViaCheckList, IRequest<string>, IIsPunchItemCommand
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public Guid GetProjectGuidForAccessCheck() => PunchItem.Project.Guid;
    public Guid GetCheckListGuidForWriteAccessCheck() => PunchItem.CheckListGuid;
    public CheckListDetailsDto CheckListDetailsDto { get; set; } = null!;
    public PunchItem PunchItem { get; set; } = null!;
    public Category Category { get; } = category;
    public string RowVersion { get; } = rowVersion;
}
