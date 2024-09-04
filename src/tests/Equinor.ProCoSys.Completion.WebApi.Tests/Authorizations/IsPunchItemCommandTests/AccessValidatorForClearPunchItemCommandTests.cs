using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ClearPunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForClearPunchItemCommandTests : AccessValidatorForCommandNeedCheckListAccessTests<ClearPunchItemCommand>
{
    protected override ClearPunchItemCommand GetCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, null!)
        {
            PunchItem = PunchItemWithAccessToProjectAndContent,
            CheckListDetailsDto = CheckListWithAccessToBothProjectAndContent
        };

    protected override ClearPunchItemCommand GetCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, null!)
        {
            PunchItem = PunchItemWithAccessToProjectButNotContent,
            CheckListDetailsDto = CheckListWithAccessToProjectButNotContent
        };

    protected override ClearPunchItemCommand GetCommandWithoutAccessToProject()
        => new(Guid.Empty, null!)
        {
            PunchItem = PunchItemWithAccessCheckListButNotProject,
            CheckListDetailsDto = CheckListWithAccessCheckListButNotProject
        };
}
