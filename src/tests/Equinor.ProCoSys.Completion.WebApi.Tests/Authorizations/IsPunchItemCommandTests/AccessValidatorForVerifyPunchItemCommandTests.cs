using Equinor.ProCoSys.Completion.Command.PunchItemCommands.VerifyPunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForVerifyPunchItemCommandTests : AccessValidatorForIIsPunchItemCommandTests<VerifyPunchItemCommand>
{
    protected override VerifyPunchItemCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(PunchItemGuidWithAccessToProjectAndContent, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override VerifyPunchItemCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(PunchItemGuidWithAccessToProjectButNotContent, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override VerifyPunchItemCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
