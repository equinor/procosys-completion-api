using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemLink;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForDeletePunchItemLinkCommandTests : AccessValidatorForCommandNeedAccessTests<DeletePunchItemLinkCommand>
{
    protected override DeletePunchItemLinkCommand GetCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, Guid.Empty, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override DeletePunchItemLinkCommand GetCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, Guid.Empty, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override DeletePunchItemLinkCommand GetCommandWithoutAccessToProject()
        => new(Guid.Empty, Guid.Empty, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
