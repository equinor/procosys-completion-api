using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemLink;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForCreatePunchItemLinkCommandTests : AccessValidatorForIIsPunchItemCommandTests<CreatePunchItemLinkCommand>
{
    protected override CreatePunchItemLinkCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(PunchItemGuidWithAccessToProjectAndContent, null!, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override CreatePunchItemLinkCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(PunchItemGuidWithAccessToProjectButNotContent, null!, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override CreatePunchItemLinkCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, null!, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
