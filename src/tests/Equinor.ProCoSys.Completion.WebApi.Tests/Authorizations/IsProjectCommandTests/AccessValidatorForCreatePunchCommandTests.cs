using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsProjectCommandTests;

[TestClass]
public class AccessValidatorForCreatePunchItemCommandTests : AccessValidatorForIIsProjectCommandTests<CreatePunchItemCommand>
{
    protected override CreatePunchItemCommand GetProjectRequestWithAccessToProjectToTest()
        => new(null!, ProjectGuidWithAccess);

    protected override CreatePunchItemCommand GetProjectRequestWithoutAccessToProjectToTest()
        => new(null!, ProjectGuidWithoutAccess);
}
