using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemLink;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForDeletePunchItemLinkCommandTests : AccessValidatorForIIsPunchItemCommandTests<DeletePunchItemLinkCommand>
{
    protected override DeletePunchItemLinkCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, Guid.Empty, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override DeletePunchItemLinkCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, Guid.Empty, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override DeletePunchItemLinkCommand GetPunchItemCommandWithoutAccessToProject()
        => new(Guid.Empty, Guid.Empty, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
