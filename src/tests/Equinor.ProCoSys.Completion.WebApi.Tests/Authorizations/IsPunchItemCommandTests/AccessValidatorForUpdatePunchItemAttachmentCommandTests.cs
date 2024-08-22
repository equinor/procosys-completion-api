using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemAttachment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForUpdatePunchItemAttachmentCommandTests : AccessValidatorForCommandNeedCheckListAccessTests<UpdatePunchItemAttachmentCommand>
{
    protected override UpdatePunchItemAttachmentCommand GetCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, Guid.Empty, "a", null!, null!)
        {
            PunchItem = PunchItemWithAccessToProjectAndContent,
            CheckListDetailsDto = CheckListWithAccessToBothProjectAndContent
        };

    protected override UpdatePunchItemAttachmentCommand GetCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, Guid.Empty, "a", null!, null!)
        {
            PunchItem = PunchItemWithAccessToProjectButNotContent,
            CheckListDetailsDto = CheckListWithAccessToProjectButNotContent
        };

    protected override UpdatePunchItemAttachmentCommand GetCommandWithoutAccessToProject()
        => new(Guid.Empty, Guid.Empty, "a", null!, null!)
        {
            PunchItem = PunchItemWithAccessCheckListButNotProject,
            CheckListDetailsDto = CheckListWithAccessCheckListButNotProject
        };
}
