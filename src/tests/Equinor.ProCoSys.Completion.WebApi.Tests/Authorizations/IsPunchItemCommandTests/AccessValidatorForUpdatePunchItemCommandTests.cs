using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForUpdatePunchItemCommandTests : AccessValidatorForCommandNeedCheckListAccessTests<UpdatePunchItemCommand>
{
    protected override UpdatePunchItemCommand GetCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, null!, null!)
        {
            PunchItem = PunchItemWithAccessToProjectAndContent,
            CheckListDetailsDto = CheckListWithAccessToBothProjectAndContent
        };

    protected override UpdatePunchItemCommand GetCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, null!, null!)
        {
            PunchItem = PunchItemWithAccessToProjectButNotContent,
            CheckListDetailsDto = CheckListWithAccessToProjectButNotContent
        };

    protected override UpdatePunchItemCommand GetCommandWithoutAccessToProject()
        => new(Guid.Empty, null!, null!)
        {
            PunchItem = PunchItemWithAccessCheckListButNotProject,
            CheckListDetailsDto = CheckListWithAccessCheckListButNotProject
        };
}
