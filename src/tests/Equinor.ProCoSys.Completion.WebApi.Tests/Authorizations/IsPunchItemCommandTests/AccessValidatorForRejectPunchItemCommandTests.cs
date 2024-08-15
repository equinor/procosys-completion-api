using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.RejectPunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForRejectPunchItemCommandTests : AccessValidatorForIIsPunchItemCommandTests<RejectPunchItemCommand>
{
    protected override RejectPunchItemCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, null!, null!, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override RejectPunchItemCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, null!, null!, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override RejectPunchItemCommand GetPunchItemCommandWithoutAccessToProject()
        => new(Guid.Empty, null!, null!, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
