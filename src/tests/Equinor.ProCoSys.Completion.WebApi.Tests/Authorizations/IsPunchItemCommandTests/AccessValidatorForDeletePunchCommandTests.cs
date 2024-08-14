using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForDeletePunchItemCommandTests : AccessValidatorForIIsPunchItemCommandTests<DeletePunchItemCommand>
{
    protected override DeletePunchItemCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(PunchItemGuidWithAccessToProjectAndContent, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override DeletePunchItemCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(PunchItemGuidWithAccessToProjectButNotContent, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override DeletePunchItemCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
