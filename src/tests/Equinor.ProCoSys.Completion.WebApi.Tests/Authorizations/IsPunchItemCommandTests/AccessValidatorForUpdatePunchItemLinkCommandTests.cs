using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemLink;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForUpdatePunchItemLinkCommandTests : AccessValidatorForCommandNeedAccessTests<UpdatePunchItemLinkCommand>
{
    protected override UpdatePunchItemLinkCommand GetCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, Guid.Empty, null!, null!, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override UpdatePunchItemLinkCommand GetCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, Guid.Empty, null!, null!, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override UpdatePunchItemLinkCommand GetCommandWithoutAccessToProject()
        => new(Guid.Empty, Guid.Empty, null!, null!, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
