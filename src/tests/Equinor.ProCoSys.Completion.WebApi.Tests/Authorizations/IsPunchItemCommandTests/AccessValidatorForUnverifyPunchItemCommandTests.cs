using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnverifyPunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForUnverifyPunchItemCommandTests : AccessValidatorForCommandNeedCheckListAccessTests<UnverifyPunchItemCommand>
{
    protected override UnverifyPunchItemCommand GetCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, null!)
        {
            PunchItem = PunchItemWithAccessToProjectAndContent,
            CheckListDetailsDto = CheckListWithAccessToBothProjectAndContent
        };

    protected override UnverifyPunchItemCommand GetCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, null!)
        {
            PunchItem = PunchItemWithAccessToProjectButNotContent,
            CheckListDetailsDto = CheckListWithAccessToProjectButNotContent
        };

    protected override UnverifyPunchItemCommand GetCommandWithoutAccessToProject()
        => new(Guid.Empty, null!)
        {
            PunchItem = PunchItemWithAccessCheckListButNotProject,
            CheckListDetailsDto = CheckListWithAccessCheckListButNotProject
        };
}
