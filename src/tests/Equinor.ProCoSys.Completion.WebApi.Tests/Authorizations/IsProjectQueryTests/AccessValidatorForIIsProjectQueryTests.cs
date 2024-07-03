using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsProjectQueryTests;

[TestClass]
public abstract class AccessValidatorForIIsProjectQueryTests<TProjectQuery> : AccessValidatorTestBase
    where TProjectQuery : IBaseRequest, IIsProjectQuery
{
    protected abstract TProjectQuery GetProjectQueryWithAccessToProjectToTest();
    protected abstract TProjectQuery GetProjectQueryWithoutAccessToProjectToTest();

    [TestMethod]
    public async Task Validate_ShouldReturnTrue_WhenAccessToProject()
    {
        // Arrange
        var query = GetProjectQueryWithAccessToProjectToTest();

        // act
        var result = await _dut.ValidateAsync(query, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Validate_ShouldReturnFalse_WhenNoAccessToProject()
    {
        // Arrange
        var query = GetProjectQueryWithoutAccessToProjectToTest();

        // act
        var result = await _dut.ValidateAsync(query, default);

        // Assert
        Assert.IsFalse(result);
    }
}
