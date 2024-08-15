using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemAttachment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForUpdatePunchItemAttachmentCommandTests : AccessValidatorForIIsPunchItemCommandTests<UpdatePunchItemAttachmentCommand>
{
    protected override UpdatePunchItemAttachmentCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, Guid.Empty, "a", null!, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override UpdatePunchItemAttachmentCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, Guid.Empty, "a", null!, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override UpdatePunchItemAttachmentCommand GetPunchItemCommandWithoutAccessToProject()
        => new(Guid.Empty, Guid.Empty, "a", null!, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
