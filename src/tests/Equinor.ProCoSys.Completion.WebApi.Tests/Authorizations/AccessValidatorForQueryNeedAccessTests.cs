using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations;

[TestClass]
public abstract class AccessValidatorForQueryNeedAccessTests<TQuery> : AccessValidatorTestBase
    where TQuery : NeedProjectAccess, IBaseRequest
{
    protected abstract TQuery GetQueryWithAccessToProjectToTest();
    protected abstract TQuery GetQueryWithoutAccessToProjectToTest();

    [TestMethod]
    public async Task Validate_ShouldReturnTrue_WhenAccessToProject()
    {
        // Arrange
        var query = GetQueryWithAccessToProjectToTest();

        // act
        var result = await _dut.ValidateAsync(query, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Validate_ShouldReturnFalse_WhenNoAccessToProject()
    {
        // Arrange
        var query = GetQueryWithoutAccessToProjectToTest();

        // act
        var result = await _dut.ValidateAsync(query, default);

        // Assert
        Assert.IsFalse(result);
    }
}
