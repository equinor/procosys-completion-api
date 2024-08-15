using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForUpdatePunchItemCommandTests : AccessValidatorForIIsPunchItemCommandTests<UpdatePunchItemCommand>
{
    protected override UpdatePunchItemCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, null!, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override UpdatePunchItemCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, null!, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override UpdatePunchItemCommand GetPunchItemCommandWithoutAccessToProject()
        => new(Guid.Empty, null!, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
