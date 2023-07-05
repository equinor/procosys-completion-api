using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ClearPunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForClearPunchItemCommandTests : AccessValidatorForIIsPunchItemCommandTests<ClearPunchItemCommand>
{
    protected override ClearPunchItemCommand GetPunchItemCommandWithAccessToProject()
        => new(PunchItemGuidWithAccessToProject, null!);

    protected override ClearPunchItemCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, null!);
}
