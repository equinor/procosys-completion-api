using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemAttachment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForDeletePunchItemAttachmentCommandTests : AccessValidatorForCommandNeedAccessTests<DeletePunchItemAttachmentCommand>
{
    protected override DeletePunchItemAttachmentCommand GetCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, Guid.Empty, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override DeletePunchItemAttachmentCommand GetCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, Guid.Empty, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override DeletePunchItemAttachmentCommand GetCommandWithoutAccessToProject()
        => new(Guid.Empty, Guid.Empty, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
