using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsProjectCommandTests;

[TestClass]
public abstract class AccessValidatorForIIsProjectCommandTests<TProjectCommand> : AccessValidatorTestBase
    where TProjectCommand : IBaseRequest, IIsProjectCommand
{
    protected abstract TProjectCommand GetProjectCommandWithAccessToBothProjectAndContent();
    protected abstract TProjectCommand GetProjectCommandWithAccessToProjectButNotContent();
    protected abstract TProjectCommand GetProjectCommandWithoutAccessToProject();

    [TestMethod]
    public async Task Validate_ShouldReturnTrue_WhenAccessToBothProjectAndContent()
    {
        // Arrange
        var command = GetProjectCommandWithAccessToBothProjectAndContent();

        // act
        var result = await _dut.ValidateAsync(command, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Validate_ShouldReturnFalse_WhenNoAccessToProject()
    {
        // Arrange
        var command = GetProjectCommandWithoutAccessToProject();

        // act
        var result = await _dut.ValidateAsync(command, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task Validate_ShouldReturnFalse_WhenAccessToProjectButNotContent()
    {
        // Arrange
        var command = GetProjectCommandWithAccessToProjectButNotContent();

        // act
        var result = await _dut.ValidateAsync(command, default);

        // Assert
        Assert.IsFalse(result);
    }
}
