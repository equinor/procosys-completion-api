using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.PunchQueries;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchQueryTests;

[TestClass]
public abstract class AccessValidatorForIPunchQueryTests<TPunchQuery> : AccessValidatorTestBase
    where TPunchQuery : IBaseRequest, IIsPunchQuery
{
    protected abstract TPunchQuery GetPunchCommandWithAccessToProject();
    protected abstract TPunchQuery GetPunchCommandWithoutAccessToProject();

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
