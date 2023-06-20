using Equinor.ProCoSys.Completion.Command.PunchCommands.CreatePunch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsProjectCommandTests;

[TestClass]
public class AccessValidatorForCreatePunchCommandTests : AccessValidatorForIIsProjectCommandTests<CreatePunchCommand>
{
    protected override CreatePunchCommand GetProjectRequestWithAccessToProjectToTest()
        => new(null!, ProjectGuidWithAccess);

    protected override CreatePunchCommand GetProjectRequestWithoutAccessToProjectToTest()
        => new(null!, ProjectGuidWithoutAccess);
}
