using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemCategory;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForUpdatePunchItemCategoryCommandTests : AccessValidatorForIIsPunchItemCommandTests<UpdatePunchItemCategoryCommand>
{
    protected override UpdatePunchItemCategoryCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(PunchItemGuidWithAccessToProjectAndContent, Category.PA, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override UpdatePunchItemCategoryCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(PunchItemGuidWithAccessToProjectButNotContent, Category.PA, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override UpdatePunchItemCategoryCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, Category.PA, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
