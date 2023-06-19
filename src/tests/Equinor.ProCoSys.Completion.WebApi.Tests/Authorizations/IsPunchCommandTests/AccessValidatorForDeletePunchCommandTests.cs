using Equinor.ProCoSys.Completion.Command.PunchCommands.DeletePunch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchCommandTests;

[TestClass]
public class AccessValidatorForDeletePunchCommandTests : AccessValidatorForIIsPunchCommandTests<DeletePunchCommand>
{
    protected override DeletePunchCommand GetPunchCommandWithAccessToProject()
        => new(PunchGuidWithAccessToProject, null!);

    protected override DeletePunchCommand GetPunchCommandWithoutAccessToProject()
        => new(PunchGuidWithoutAccessToProject, null!);
}
