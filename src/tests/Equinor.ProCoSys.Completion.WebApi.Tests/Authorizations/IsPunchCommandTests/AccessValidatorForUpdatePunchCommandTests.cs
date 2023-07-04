using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchCommandTests;

[TestClass]
public class AccessValidatorForUpdatePunchItemCommandTests : AccessValidatorForIIsPunchCommandTests<UpdatePunchItemCommand>
{
    protected override UpdatePunchItemCommand GetPunchItemCommandWithAccessToProject()
        => new(PunchItemGuidWithAccessToProject, null!, null!);

    protected override UpdatePunchItemCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, null!, null!);
}
