using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations;

[TestClass]
public abstract class AccessValidatorForCommandNeedAccessTests<TCommand> : AccessValidatorTestBase
    where TCommand : NeedProjectAccess, IBaseRequest
{
    protected abstract TCommand GetCommandWithAccessToBothProjectAndContent();
    protected abstract TCommand GetCommandWithAccessToProjectButNotContent();
    protected abstract TCommand GetCommandWithoutAccessToProject();

    [TestMethod]
    public async Task Validate_ShouldReturnTrue_WhenAccessToBothProjectAndContent()
    {
        // Arrange
        var command = GetCommandWithAccessToBothProjectAndContent();

        // act
        var result = await _dut.ValidateAsync(command, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Validate_ShouldReturnFalse_WhenNoAccessToProject()
    {
        // Arrange
        var command = GetCommandWithoutAccessToProject();

        // act
        var result = await _dut.ValidateAsync(command, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task Validate_ShouldReturnFalse_WhenAccessToProjectButNotContent()
    {
        // Arrange
        var command = GetCommandWithAccessToProjectButNotContent();

        // act
        var result = await _dut.ValidateAsync(command, default);

        // Assert
        Assert.IsFalse(result);
    }
}
