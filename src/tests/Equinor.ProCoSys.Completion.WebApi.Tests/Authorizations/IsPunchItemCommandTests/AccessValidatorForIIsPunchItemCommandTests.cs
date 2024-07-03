using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public abstract class AccessValidatorForIIsPunchItemCommandTests<TPunchItemCommand> : AccessValidatorTestBase
    where TPunchItemCommand : IBaseRequest, IIsPunchItemCommand
{
    protected abstract TPunchItemCommand GetPunchItemCommandWithAccessToBothProjectAndContent();
    protected abstract TPunchItemCommand GetPunchItemCommandWithAccessToProjectButNotContent();
    protected abstract TPunchItemCommand GetPunchItemCommandWithoutAccessToProject();

    [TestMethod]
    public async Task Validate_ShouldReturnTrue_WhenAccessToBothProjectAndContent()
    {
        // Arrange
        var command = GetPunchItemCommandWithAccessToBothProjectAndContent();

        // act
        var result = await _dut.ValidateAsync(command, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Validate_ShouldReturnFalse_WhenNoAccessToProject()
    {
        // Arrange
        var command = GetPunchItemCommandWithoutAccessToProject();

        // act
        var result = await _dut.ValidateAsync(command, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task Validate_ShouldReturnFalse_WhenAccessToProjectButNotContent()
    {
        // Arrange
        var command = GetPunchItemCommandWithAccessToProjectButNotContent();

        // act
        var result = await _dut.ValidateAsync(command, default);

        // Assert
        Assert.IsFalse(result);
    }
}
