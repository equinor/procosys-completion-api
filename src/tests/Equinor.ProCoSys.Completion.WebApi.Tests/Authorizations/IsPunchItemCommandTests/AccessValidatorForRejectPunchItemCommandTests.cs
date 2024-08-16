using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.RejectPunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForRejectPunchItemCommandTests : AccessValidatorForCommandNeedAccessTests<RejectPunchItemCommand>
{
    protected override RejectPunchItemCommand GetCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, null!, null!, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override RejectPunchItemCommand GetCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, null!, null!, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override RejectPunchItemCommand GetCommandWithoutAccessToProject()
        => new(Guid.Empty, null!, null!, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
