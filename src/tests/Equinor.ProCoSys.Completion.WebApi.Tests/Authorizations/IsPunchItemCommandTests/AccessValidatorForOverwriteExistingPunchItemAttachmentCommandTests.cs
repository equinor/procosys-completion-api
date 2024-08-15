using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.OverwriteExistingPunchItemAttachment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForOverwriteExistingPunchItemAttachmentCommandTests : AccessValidatorForIIsPunchItemCommandTests<OverwriteExistingPunchItemAttachmentCommand>
{
    protected override OverwriteExistingPunchItemAttachmentCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, null!, null!, null!, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override OverwriteExistingPunchItemAttachmentCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, null!, null!, null!, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override OverwriteExistingPunchItemAttachmentCommand GetPunchItemCommandWithoutAccessToProject()
        => new(Guid.Empty, null!, null!, null!, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
