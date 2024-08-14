using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnclearPunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForUnclearPunchItemCommandTests : AccessValidatorForIIsPunchItemCommandTests<UnclearPunchItemCommand>
{
    protected override UnclearPunchItemCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(PunchItemGuidWithAccessToProjectAndContent, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override UnclearPunchItemCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(PunchItemGuidWithAccessToProjectButNotContent, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override UnclearPunchItemCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
