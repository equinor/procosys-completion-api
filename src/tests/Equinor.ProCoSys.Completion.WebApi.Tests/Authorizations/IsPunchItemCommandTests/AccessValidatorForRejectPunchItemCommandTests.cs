using Equinor.ProCoSys.Completion.Command.PunchItemCommands.RejectPunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForRejectPunchItemCommandTests : AccessValidatorForIIsPunchItemCommandTests<RejectPunchItemCommand>
{
    protected override RejectPunchItemCommand GetPunchItemCommandWithAccessToProject()
        => new(PunchItemGuidWithAccessToProject, null!);

    protected override RejectPunchItemCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, null!);
}
