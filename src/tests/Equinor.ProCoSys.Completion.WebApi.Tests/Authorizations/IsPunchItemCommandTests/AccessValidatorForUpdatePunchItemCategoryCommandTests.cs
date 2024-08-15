using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemCategory;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForUpdatePunchItemCategoryCommandTests : AccessValidatorForIIsPunchItemCommandTests<UpdatePunchItemCategoryCommand>
{
    protected override UpdatePunchItemCategoryCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, Category.PA, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override UpdatePunchItemCategoryCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, Category.PA, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override UpdatePunchItemCategoryCommand GetPunchItemCommandWithoutAccessToProject()
        => new(Guid.Empty, Category.PA, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
