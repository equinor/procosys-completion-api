using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public abstract class AccessValidatorForIIsPunchItemCommandTests<TPunchItemCommand> : AccessValidatorTestBase
    where TPunchItemCommand : IBaseRequest, IIsPunchItemCommand
{
    protected abstract TPunchItemCommand GetPunchItemCommandWithAccessToProject();
    protected abstract TPunchItemCommand GetPunchItemCommandWithoutAccessToProject();

    [TestMethod]
    public async Task Validate_ShouldReturnTrue_WhenAccessToProjectForPunchItem()
    {
        // Arrange
        var command = GetPunchItemCommandWithAccessToProject();

        // act
        var result = await _dut.ValidateAsync(command);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Validate_ShouldReturnFalse_WhenNoAccessToProjectForPunchItem()
    {
        // Arrange
        var command = GetPunchItemCommandWithoutAccessToProject();

        // act
        var result = await _dut.ValidateAsync(command);

        // Assert
        Assert.IsFalse(result);
    }
}
