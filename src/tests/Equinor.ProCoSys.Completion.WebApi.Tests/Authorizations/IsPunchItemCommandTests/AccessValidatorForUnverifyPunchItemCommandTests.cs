using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnverifyPunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForUnverifyPunchItemCommandTests : AccessValidatorForIIsPunchItemCommandTests<UnverifyPunchItemCommand>
{
    protected override UnverifyPunchItemCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override UnverifyPunchItemCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override UnverifyPunchItemCommand GetPunchItemCommandWithoutAccessToProject()
        => new(Guid.Empty, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
