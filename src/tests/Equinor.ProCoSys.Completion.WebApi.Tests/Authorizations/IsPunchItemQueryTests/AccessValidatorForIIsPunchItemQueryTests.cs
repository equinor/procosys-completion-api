using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemQueryTests;

[TestClass]
public abstract class AccessValidatorForIIsPunchItemQueryTests<TPunchItemQuery> : AccessValidatorTestBase
    where TPunchItemQuery : IBaseRequest, IIsPunchItemQuery
{
    protected abstract TPunchItemQuery GetPunchItemQueryWithAccessToProject();
    protected abstract TPunchItemQuery GetPunchItemQueryWithoutAccessToProject();

    [TestMethod]
    public async Task Validate_ShouldReturnTrue_WhenAccessToProjectForPunchItem()
    {
        // Arrange
        var command = GetPunchItemQueryWithAccessToProject();

        // act
        var result = await _dut.ValidateAsync(command);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Validate_ShouldReturnFalse_WhenNoAccessToProjectForPunchItem()
    {
        // Arrange
        var command = GetPunchItemQueryWithoutAccessToProject();

        // act
        var result = await _dut.ValidateAsync(command);

        // Assert
        Assert.IsFalse(result);
    }
}
