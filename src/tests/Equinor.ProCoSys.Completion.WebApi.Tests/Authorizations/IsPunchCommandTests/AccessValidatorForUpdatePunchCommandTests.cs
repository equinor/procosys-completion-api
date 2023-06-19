using Equinor.ProCoSys.Completion.Command.PunchCommands.UpdatePunch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchCommandTests;

[TestClass]
public class AccessValidatorForUpdatePunchCommandTests : AccessValidatorForIIsPunchCommandTests<UpdatePunchCommand>
{
    protected override UpdatePunchCommand GetPunchCommandWithAccessToProject()
        => new(PunchGuidWithAccessToProject, null!, null!, null!);

    protected override UpdatePunchCommand GetPunchCommandWithoutAccessToProject()
        => new(PunchGuidWithoutAccessToProject, null!, null!, null!);
}
