using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForUpdatePunchItemCommandTests : AccessValidatorForIIsPunchItemCommandTests<UpdatePunchItemCommand>
{
    protected override UpdatePunchItemCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(PunchItemGuidWithAccessToProjectAndContent, null!);

    protected override UpdatePunchItemCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(PunchItemGuidWithAccessToProjectButNotContent, null!);

    protected override UpdatePunchItemCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, null!);
}
