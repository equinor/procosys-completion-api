using Equinor.ProCoSys.Completion.Command.PunchItemCommands.VerifyPunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForVerifyPunchItemCommandTests : AccessValidatorForIIsPunchItemCommandTests<VerifyPunchItemCommand>
{
    protected override VerifyPunchItemCommand GetPunchItemCommandWithAccessToProject()
        => new(PunchItemGuidWithAccessToProject, null!);

    protected override VerifyPunchItemCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, null!);
}
