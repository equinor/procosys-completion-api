using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsProjectCommandTests;

[TestClass]
public class AccessValidatorForCreatePunchItemCommandTests : AccessValidatorForIIsProjectCommandTests<CreatePunchItemCommand>
{
    protected override CreatePunchItemCommand GetProjectCommandWithAccessToBothProjectAndContent()
        => new(
            Category.PA,
            null!,
            ProjectGuidWithAccess,
            CheckListGuidWithAccessToContent,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            false,
            null,
            null);

    protected override CreatePunchItemCommand GetProjectCommandWithAccessToProjectButNotContent()
        => new(Category.PA, 
            null!, 
            ProjectGuidWithAccess, 
            CheckListGuidWithoutAccessToContent,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            false,
            null,
            null);

    protected override CreatePunchItemCommand GetProjectCommandWithoutAccessToProject()
        => new(Category.PA,
            null!,
            ProjectGuidWithoutAccess,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            false,
            null,
            null);
}
