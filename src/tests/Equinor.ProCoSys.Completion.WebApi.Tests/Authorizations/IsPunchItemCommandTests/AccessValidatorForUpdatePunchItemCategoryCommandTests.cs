using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemCategory;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForUpdatePunchItemCategoryCommandTests : AccessValidatorForCommandNeedAccessTests<UpdatePunchItemCategoryCommand>
{
    protected override UpdatePunchItemCategoryCommand GetCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, Category.PA, null!)
        {
            PunchItem = PunchItemWithAccessToProjectAndContent,
            CheckListDetailsDto = CheckListWithAccessToBothProjectAndContent
        };

    protected override UpdatePunchItemCategoryCommand GetCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, Category.PA, null!)
        {
            PunchItem = PunchItemWithAccessToProjectButNotContent,
            CheckListDetailsDto = CheckListWithAccessToProjectButNotContent
        };

    protected override UpdatePunchItemCategoryCommand GetCommandWithoutAccessToProject()
        => new(Guid.Empty, Category.PA, null!)
        {
            PunchItem = PunchItemWithAccessCheckListButNotProject,
            CheckListDetailsDto = CheckListWithAccessCheckListButNotProject
        };
}
