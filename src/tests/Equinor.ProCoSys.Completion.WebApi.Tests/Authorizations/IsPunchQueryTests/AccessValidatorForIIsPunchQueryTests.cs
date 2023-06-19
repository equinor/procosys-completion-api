using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.PunchQueries;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchQueryTests;

[TestClass]
public abstract class AccessValidatorForIIsPunchQueryTests<TPunchQuery> : AccessValidatorTestBase
    where TPunchQuery : IBaseRequest, IIsPunchQuery
{
    protected abstract TPunchQuery GetPunchQueryWithAccessToProject();
    protected abstract TPunchQuery GetPunchQueryWithoutAccessToProject();

    [TestMethod]
    public async Task Validate_ShouldReturnTrue_WhenAccessToProjectForPunch()
    {
        // Arrange
        var command = GetPunchQueryWithAccessToProject();

        // act
        var result = await _dut.ValidateAsync(command);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Validate_ShouldReturnFalse_WhenNoAccessToProjectForPunch()
    {
        // Arrange
        var command = GetPunchQueryWithoutAccessToProject();

        // act
        var result = await _dut.ValidateAsync(command);

        // Assert
        Assert.IsFalse(result);
    }
}
