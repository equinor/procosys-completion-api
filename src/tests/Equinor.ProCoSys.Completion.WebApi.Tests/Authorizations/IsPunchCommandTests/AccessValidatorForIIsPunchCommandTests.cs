using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchCommandTests;

[TestClass]
public abstract class AccessValidatorForIIsPunchCommandTests<TPunchCommand> : AccessValidatorTestBase
    where TPunchCommand : IBaseRequest, IIsPunchCommand
{
    protected abstract TPunchCommand GetPunchCommandWithAccessToProject();
    protected abstract TPunchCommand GetPunchCommandWithoutAccessToProject();

    [TestMethod]
    public async Task Validate_ShouldReturnTrue_WhenAccessToProjectForPunch()
    {
        // Arrange
        var command = GetPunchCommandWithAccessToProject();

        // act
        var result = await _dut.ValidateAsync(command);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Validate_ShouldReturnFalse_WhenNoAccessToProjectForPunch()
    {
        // Arrange
        var command = GetPunchCommandWithoutAccessToProject();

        // act
        var result = await _dut.ValidateAsync(command);

        // Assert
        Assert.IsFalse(result);
    }
}
