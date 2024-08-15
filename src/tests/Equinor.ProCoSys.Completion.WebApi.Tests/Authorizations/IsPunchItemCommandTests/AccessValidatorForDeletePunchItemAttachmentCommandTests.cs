using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemAttachment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForDeletePunchItemAttachmentCommandTests : AccessValidatorForIIsPunchItemCommandTests<DeletePunchItemAttachmentCommand>
{
    protected override DeletePunchItemAttachmentCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, Guid.Empty, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override DeletePunchItemAttachmentCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, Guid.Empty, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override DeletePunchItemAttachmentCommand GetPunchItemCommandWithoutAccessToProject()
        => new(Guid.Empty, Guid.Empty, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
