using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;
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
            null)
        {
            CheckListDetailsDto =
                new CheckListDetailsDto(CheckListGuidWithAccessToContent, "R", false, ProjectGuidWithAccess)
        };

    protected override CreatePunchItemCommand GetCommandWithAccessToProjectButNotContent()
        => new(Category.PA, 
            null!, 
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
            null)
        {
            CheckListDetailsDto =
                new CheckListDetailsDto(CheckListGuidWithoutAccessToContent, "R", false, ProjectGuidWithAccess)
        };

    protected override CreatePunchItemCommand GetCommandWithoutAccessToProject()
        => new(Category.PA,
            null!,
            ProjectGuidWithoutAccess,
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
            null)
        {
            CheckListDetailsDto =
                new CheckListDetailsDto(Guid.NewGuid(), "R", false, ProjectGuidWithoutAccess)
        };
}
