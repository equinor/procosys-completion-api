using Equinor.ProCoSys.Completion.Domain;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations;

[TestClass]
public abstract class AccessValidatorForCommandNeedAccessTests<TCommand> : AccessValidatorTestBase
    where TCommand : INeedProjectAccess, IBaseRequest
{
    protected abstract TCommand GetCommandWithAccessToBothProjectAndContent();
    protected abstract TCommand GetCommandWithAccessToProjectButNotContent();
    protected abstract TCommand GetCommandWithoutAccessToProject();

    [TestMethod]
    public void Validate_ShouldReturnTrue_WhenAccessToBothProjectAndContent()
    {
        // Arrange
        var command = GetCommandWithAccessToBothProjectAndContent();

        // act
        var result = _dut.HasAccess(command);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Validate_ShouldReturnFalse_WhenNoAccessToProject()
    {
        // Arrange
        var command = GetCommandWithoutAccessToProject();

        // act
        var result = _dut.HasAccess(command);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Validate_ShouldReturnFalse_WhenAccessToProjectButNotContent()
    {
        // Arrange
        var command = GetCommandWithAccessToProjectButNotContent();

        // act
        var result = _dut.HasAccess(command);

        // Assert
        Assert.IsFalse(result);
    }
}
