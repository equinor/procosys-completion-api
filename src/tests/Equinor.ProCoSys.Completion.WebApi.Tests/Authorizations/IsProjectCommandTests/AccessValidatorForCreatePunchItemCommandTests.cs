using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsProjectCommandTests;

[TestClass]
public class AccessValidatorForCreatePunchItemCommandTests : AccessValidatorForCommandNeedAccessTests<CreatePunchItemCommand>
{
    protected override CreatePunchItemCommand GetCommandWithAccessToBothProjectAndContent()
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

    protected override CreatePunchItemCommand GetCommandWithAccessToProjectButNotContent()
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

    protected override CreatePunchItemCommand GetCommandWithoutAccessToProject()
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
