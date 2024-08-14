using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ClearPunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForClearPunchItemCommandTests : AccessValidatorForIIsPunchItemCommandTests<ClearPunchItemCommand>
{
    protected override ClearPunchItemCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(PunchItemGuidWithAccessToProjectAndContent, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override ClearPunchItemCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(PunchItemGuidWithAccessToProjectButNotContent, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override ClearPunchItemCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
