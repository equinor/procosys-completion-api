using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.VerifyPunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForVerifyPunchItemCommandTests : AccessValidatorForCommandNeedCheckListAccessTests<VerifyPunchItemCommand>
{
    protected override VerifyPunchItemCommand GetCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, null!)
        {
            PunchItem = PunchItemWithAccessToProjectAndContent,
            CheckListDetailsDto = CheckListWithAccessToBothProjectAndContent
        };

    protected override VerifyPunchItemCommand GetCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, null!)
        {
            PunchItem = PunchItemWithAccessToProjectButNotContent,
            CheckListDetailsDto = CheckListWithAccessToProjectButNotContent
        };

    protected override VerifyPunchItemCommand GetCommandWithoutAccessToProject()
        => new(Guid.Empty, null!)
        {
            PunchItem = PunchItemWithAccessCheckListButNotProject,
            CheckListDetailsDto = CheckListWithAccessCheckListButNotProject
        };
}
