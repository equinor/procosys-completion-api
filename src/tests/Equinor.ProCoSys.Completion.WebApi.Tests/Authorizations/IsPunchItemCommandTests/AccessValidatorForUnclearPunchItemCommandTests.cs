using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnclearPunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForUnclearPunchItemCommandTests : AccessValidatorForCommandNeedAccessTests<UnclearPunchItemCommand>
{
    protected override UnclearPunchItemCommand GetCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, null!)
        {
            PunchItem = PunchItemWithAccessToProjectAndContent,
            CheckListDetailsDto = CheckListWithAccessToBothProjectAndContent
        };

    protected override UnclearPunchItemCommand GetCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, null!)
        {
            PunchItem = PunchItemWithAccessToProjectButNotContent,
            CheckListDetailsDto = CheckListWithAccessToProjectButNotContent
        };

    protected override UnclearPunchItemCommand GetCommandWithoutAccessToProject()
        => new(Guid.Empty, null!)
        {
            PunchItem = PunchItemWithAccessCheckListButNotProject,
            CheckListDetailsDto = CheckListWithAccessCheckListButNotProject
        };
}
