using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnclearPunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForUnclearPunchItemCommandTests : AccessValidatorForIIsPunchItemCommandTests<UnclearPunchItemCommand>
{
    protected override UnclearPunchItemCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override UnclearPunchItemCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override UnclearPunchItemCommand GetPunchItemCommandWithoutAccessToProject()
        => new(Guid.Empty, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
