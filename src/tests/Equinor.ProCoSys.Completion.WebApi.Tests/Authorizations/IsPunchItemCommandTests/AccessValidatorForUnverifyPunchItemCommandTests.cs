using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnverifyPunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForUnverifyPunchItemCommandTests : AccessValidatorForIIsPunchItemCommandTests<UnverifyPunchItemCommand>
{
    protected override UnverifyPunchItemCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(PunchItemGuidWithAccessToProjectAndContent, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override UnverifyPunchItemCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(PunchItemGuidWithAccessToProjectButNotContent, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override UnverifyPunchItemCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
