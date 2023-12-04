using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemCategory;

public class UpdatePunchItemCategoryCommand : IRequest<Result<string>>, IIsPunchItemCommand
{
    public UpdatePunchItemCategoryCommand(Guid punchItemGuid, Category category, string rowVersion)
    {
        PunchItemGuid = punchItemGuid;
        Category = category;
        RowVersion = rowVersion;
    }

    public Guid PunchItemGuid { get; }
    public Category Category { get; }
    public string RowVersion { get; }
}
