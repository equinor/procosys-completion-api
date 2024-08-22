using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DuplicatePunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForDuplicatePunchItemCommandTests : AccessValidatorForCommandNeedManyCheckListAccessTests<DuplicatePunchItemCommand>
{
    protected override DuplicatePunchItemCommand GetCommandWithAccessToBothProjectAndContent()
        => new(PunchItemWithAccessToProjectAndContent.Guid, [CheckListWithAccessToBothProjectAndContent.CheckListGuid], false)
        {
            PunchItem = PunchItemWithAccessToProjectAndContent,
            CheckListDetailsDtoList = [CheckListWithAccessToBothProjectAndContent]
        };

    protected override DuplicatePunchItemCommand GetCommandWithAccessToProjectButNotContent()
        => new(PunchItemWithAccessToProjectButNotContent.Guid, [CheckListWithAccessToProjectButNotContent.CheckListGuid], false)
        {
            PunchItem = PunchItemWithAccessToProjectButNotContent,
            CheckListDetailsDtoList = [CheckListWithAccessToProjectButNotContent]
        };

    protected override DuplicatePunchItemCommand GetCommandWithoutAccessToProject()
        => new(PunchItemWithAccessCheckListButNotProject.Guid,[CheckListWithAccessCheckListButNotProject.CheckListGuid], false)
        {
            PunchItem = PunchItemWithAccessCheckListButNotProject,
            CheckListDetailsDtoList = [CheckListWithAccessCheckListButNotProject]
        };
}
