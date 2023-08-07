using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsProjectCommandTests;

[TestClass]
public abstract class AccessValidatorForIIsProjectCommandTests<TProjectCommand> : AccessValidatorTestBase
    where TProjectCommand : IBaseRequest, IIsProjectCommand
{
    protected abstract TProjectCommand GetProjectCommandWithAccessToProjectToTest();
    protected abstract TProjectCommand GetProjectCommandWithoutAccessToProjectToTest();

    [TestMethod]
    public async Task Validate_ShouldReturnTrue_WhenAccessToProject()
    {
        // Arrange
        var command = GetProjectCommandWithAccessToProjectToTest();

        // act
        var result = await _dut.ValidateAsync(command);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Validate_ShouldReturnFalse_WhenNoAccessToProject()
    {
        // Arrange
        var command = GetProjectCommandWithoutAccessToProjectToTest();

        // act
        var result = await _dut.ValidateAsync(command);

        // Assert
        Assert.IsFalse(result);
    }
}
