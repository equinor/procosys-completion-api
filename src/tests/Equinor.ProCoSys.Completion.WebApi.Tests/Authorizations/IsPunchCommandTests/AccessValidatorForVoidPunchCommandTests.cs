using Equinor.ProCoSys.Completion.Command.PunchCommands.VoidPunch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchCommandTests;

[TestClass]
public class AccessValidatorForVoidPunchCommandTests : AccessValidatorForIIsPunchCommandTests<VoidPunchCommand>
{
    protected override VoidPunchCommand GetPunchCommandWithAccessToProject()
        => new(PunchGuidWithAccessToProject, null!);

    protected override VoidPunchCommand GetPunchCommandWithoutAccessToProject()
        => new(PunchGuidWithoutAccessToProject, null!);
}
