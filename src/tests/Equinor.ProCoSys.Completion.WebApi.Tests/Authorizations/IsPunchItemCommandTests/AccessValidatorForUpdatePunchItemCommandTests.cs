using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForUpdatePunchItemCommandTests : AccessValidatorForCommandNeedAccessTests<UpdatePunchItemCommand>
{
    protected override UpdatePunchItemCommand GetCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, null!, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override UpdatePunchItemCommand GetCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, null!, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override UpdatePunchItemCommand GetCommandWithoutAccessToProject()
        => new(Guid.Empty, null!, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
