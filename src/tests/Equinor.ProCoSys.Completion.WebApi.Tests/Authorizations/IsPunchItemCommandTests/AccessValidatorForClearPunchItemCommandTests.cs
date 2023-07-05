using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForUpdatePunchItemCommandTests : AccessValidatorForIIsPunchItemCommandTests<UpdatePunchItemCommand>
{
    protected override UpdatePunchItemCommand GetPunchItemCommandWithAccessToProject()
        => new(PunchItemGuidWithAccessToProject, null!, null!);

    protected override UpdatePunchItemCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, null!, null!);
}
