using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UploadNewPunchItemAttachment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForUploadNewPunchItemAttachmentCommandTests : AccessValidatorForIIsPunchItemCommandTests<UploadNewPunchItemAttachmentCommand>
{
    protected override UploadNewPunchItemAttachmentCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, null!, null!, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override UploadNewPunchItemAttachmentCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, null!, null!, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override UploadNewPunchItemAttachmentCommand GetPunchItemCommandWithoutAccessToProject()
        => new(Guid.Empty, null!, null!, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
