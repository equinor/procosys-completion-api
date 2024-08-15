using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemAttachment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForUpdatePunchItemAttachmentCommandTests : AccessValidatorForCommandNeedAccessTests<UpdatePunchItemAttachmentCommand>
{
    protected override UpdatePunchItemAttachmentCommand GetCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, Guid.Empty, "a", null!, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override UpdatePunchItemAttachmentCommand GetCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, Guid.Empty, "a", null!, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override UpdatePunchItemAttachmentCommand GetCommandWithoutAccessToProject()
        => new(Guid.Empty, Guid.Empty, "a", null!, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
