using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UploadNewPunchItemAttachment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForUploadNewPunchItemAttachmentCommandTests : AccessValidatorForCommandNeedCheckListAccessTests<UploadNewPunchItemAttachmentCommand>
{
    protected override UploadNewPunchItemAttachmentCommand GetCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, null!, null!, null!)
        {
            PunchItem = PunchItemWithAccessToProjectAndContent,
            CheckListDetailsDto = CheckListWithAccessToBothProjectAndContent
        };

    protected override UploadNewPunchItemAttachmentCommand GetCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, null!, null!, null!)
        {
            PunchItem = PunchItemWithAccessToProjectButNotContent,
            CheckListDetailsDto = CheckListWithAccessToProjectButNotContent
        };

    protected override UploadNewPunchItemAttachmentCommand GetCommandWithoutAccessToProject()
        => new(Guid.Empty, null!, null!, null!)
        {
            PunchItem = PunchItemWithAccessCheckListButNotProject,
            CheckListDetailsDto = CheckListWithAccessCheckListButNotProject
        };
}
