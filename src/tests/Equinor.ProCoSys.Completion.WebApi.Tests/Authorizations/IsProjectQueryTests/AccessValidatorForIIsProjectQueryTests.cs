using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsProjectQueryTests;

[TestClass]
public abstract class AccessValidatorForIIsProjectQueryTests<TProjectQuery> : AccessValidatorTestBase
    where TProjectQuery : IBaseRequest, IIsProjectQuery
{
    protected abstract TProjectQuery GetProjectRequestWithAccessToProjectToTest();
    protected abstract TProjectQuery GetProjectRequestWithoutAccessToProjectToTest();

    [TestMethod]
    public async Task Validate_ShouldReturnTrue_WhenAccessToProject()
    {
        // Arrange
        var query = GetProjectRequestWithAccessToProjectToTest();

        // act
        var result = await _dut.ValidateAsync(query);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Validate_ShouldReturnFalse_WhenNoAccessToProject()
    {
        // Arrange
        var query = GetProjectRequestWithoutAccessToProjectToTest();

        // act
        var result = await _dut.ValidateAsync(query);

        // Assert
        Assert.IsFalse(result);
    }
}
