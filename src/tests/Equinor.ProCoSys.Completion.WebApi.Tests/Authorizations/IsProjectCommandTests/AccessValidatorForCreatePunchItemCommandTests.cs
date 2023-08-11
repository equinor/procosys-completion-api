using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsProjectCommandTests;

[TestClass]
public class AccessValidatorForCreatePunchItemCommandTests : AccessValidatorForIIsProjectCommandTests<CreatePunchItemCommand>
{
    protected override CreatePunchItemCommand GetProjectCommandWithAccessToProjectAndContent()
        => new(null!, ProjectGuidWithAccess, CheckListGuidWithAccessToContent, Guid.Empty, Guid.Empty);

    protected override CreatePunchItemCommand GetProjectCommandWithoutAccessToProject()
        => new(null!, ProjectGuidWithoutAccess, Guid.Empty, Guid.Empty, Guid.Empty);

    [TestMethod]
    public async Task Validate_ShouldReturnFalse_WhenAccessToProjectButNotContent()
    {
        // Arrange
        var command = new CreatePunchItemCommand(
            null!,
            ProjectGuidWithAccess,
            CheckListGuidWithoutAccessToContent,
            Guid.Empty,
            Guid.Empty);

        // act
        var result = await _dut.ValidateAsync(command);

        // Assert
        Assert.IsFalse(result);
    }
}
