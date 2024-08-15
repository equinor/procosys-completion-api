using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForDeletePunchItemCommandTests : AccessValidatorForIIsPunchItemCommandTests<DeletePunchItemCommand>
{
    protected override DeletePunchItemCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override DeletePunchItemCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override DeletePunchItemCommand GetPunchItemCommandWithoutAccessToProject()
        => new(Guid.Empty, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
