using Equinor.ProCoSys.Completion.Command.PunchItemCommands.RejectPunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForRejectPunchItemCommandTests : AccessValidatorForIIsPunchItemCommandTests<RejectPunchItemCommand>
{
    protected override RejectPunchItemCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(PunchItemGuidWithAccessToProjectAndContent, null!, null!);

    protected override RejectPunchItemCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(PunchItemGuidWithAccessToProjectButNotContent, null!, null!);

    protected override RejectPunchItemCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, null!, null!);
}
