using System;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemCategory;

public class UpdatePunchItemCategoryCommand(Guid punchItemGuid, Category category, string rowVersion)
    : CanHaveRestrictionsViaCheckList, IRequest<Result<string>>, IIsPunchItemCommand
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public override Guid GetProjectGuidForAccessCheck() => PunchItem.Project.Guid;
    public override Guid GetCheckListGuidForWriteAccessCheck() => PunchItem.CheckListGuid;
    public PunchItem PunchItem { get; set; } = null!;
    public Category Category { get; } = category;
    public string RowVersion { get; } = rowVersion;
}
