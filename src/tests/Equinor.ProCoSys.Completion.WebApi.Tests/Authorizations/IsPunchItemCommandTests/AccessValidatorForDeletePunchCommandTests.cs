using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForDeletePunchItemCommandTests : AccessValidatorForCommandNeedAccessTests<DeletePunchItemCommand>
{
    protected override DeletePunchItemCommand GetCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override DeletePunchItemCommand GetCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override DeletePunchItemCommand GetCommandWithoutAccessToProject()
        => new(Guid.Empty, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
