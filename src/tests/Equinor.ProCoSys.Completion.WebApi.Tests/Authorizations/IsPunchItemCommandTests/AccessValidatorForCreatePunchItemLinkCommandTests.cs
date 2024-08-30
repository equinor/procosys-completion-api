using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemLink;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForCreatePunchItemLinkCommandTests : AccessValidatorForCommandNeedCheckListAccessTests<CreatePunchItemLinkCommand>
{
    protected override CreatePunchItemLinkCommand GetCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, null!, null!)
        {
            PunchItem = PunchItemWithAccessToProjectAndContent,
            CheckListDetailsDto = CheckListWithAccessToBothProjectAndContent
        };

    protected override CreatePunchItemLinkCommand GetCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, null!, null!)
        {
            PunchItem = PunchItemWithAccessToProjectButNotContent,
            CheckListDetailsDto = CheckListWithAccessToProjectButNotContent
        };

    protected override CreatePunchItemLinkCommand GetCommandWithoutAccessToProject()
        => new(Guid.Empty, null!, null!)
        {
            PunchItem = PunchItemWithAccessCheckListButNotProject,
            CheckListDetailsDto = CheckListWithAccessCheckListButNotProject
        };
}
