using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemLink;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForDeletePunchItemCommandTests : AccessValidatorForIIsPunchItemCommandTests<DeletePunchItemCommand>
{
    protected override DeletePunchItemCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(PunchItemGuidWithAccessToProjectAndContent, null!);

    protected override DeletePunchItemCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(PunchItemGuidWithAccessToProjectButNotContent, null!);

    protected override DeletePunchItemCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, null!);
}
