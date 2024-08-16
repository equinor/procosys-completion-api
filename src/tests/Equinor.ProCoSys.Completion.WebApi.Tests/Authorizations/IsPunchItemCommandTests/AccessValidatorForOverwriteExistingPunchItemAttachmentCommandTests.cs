using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.OverwriteExistingPunchItemAttachment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForOverwriteExistingPunchItemAttachmentCommandTests : AccessValidatorForCommandNeedAccessTests<OverwriteExistingPunchItemAttachmentCommand>
{
    protected override OverwriteExistingPunchItemAttachmentCommand GetCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, null!, null!, null!, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override OverwriteExistingPunchItemAttachmentCommand GetCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, null!, null!, null!, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override OverwriteExistingPunchItemAttachmentCommand GetCommandWithoutAccessToProject()
        => new(Guid.Empty, null!, null!, null!, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
