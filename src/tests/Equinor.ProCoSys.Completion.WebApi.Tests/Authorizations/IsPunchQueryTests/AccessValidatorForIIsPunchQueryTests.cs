using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchQueryTests;

[TestClass]
public abstract class AccessValidatorForIIsPunchQueryTests<TPunchQuery> : AccessValidatorTestBase
    where TPunchQuery : IBaseRequest, IIsPunchItemQuery
{
    protected abstract TPunchQuery GetPunchItemQueryWithAccessToProject();
    protected abstract TPunchQuery GetPunchItemQueryWithoutAccessToProject();

    [TestMethod]
    public async Task Validate_ShouldReturnTrue_WhenAccessToProjectForPunch()
    {
        // Arrange
        var command = GetPunchItemQueryWithAccessToProject();

        // act
        var result = await _dut.ValidateAsync(command);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Validate_ShouldReturnFalse_WhenNoAccessToProjectForPunch()
    {
        // Arrange
        var command = GetPunchItemQueryWithoutAccessToProject();

        // act
        var result = await _dut.ValidateAsync(command);

        // Assert
        Assert.IsFalse(result);
    }
}
