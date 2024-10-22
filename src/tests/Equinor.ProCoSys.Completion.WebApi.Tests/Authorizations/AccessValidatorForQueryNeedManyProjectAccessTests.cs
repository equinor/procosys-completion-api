using Equinor.ProCoSys.Completion.Domain;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations;

[TestClass]
public abstract class AccessValidatorForQueryNeedManyProjectAccessTests<TQuery> : AccessValidatorTestBase
    where TQuery : INeedProjectsAccess, IBaseRequest
{
    protected abstract TQuery GetQueryWithAccessToAllProjectsToTest();
    protected abstract TQuery GetQueryWithoutAccessToAllProjectsToTest();

    [TestMethod]
    public void Validate_ShouldReturnTrue_WhenAccessToAllProjects()
    {
        // Arrange
        var query = GetQueryWithAccessToAllProjectsToTest();

        // act
        var result = _dut.HasAccess(query);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Validate_ShouldReturnFalse_WhenNotAccessToAllProjects()
    {
        // Arrange
        var query = GetQueryWithoutAccessToAllProjectsToTest();

        // act
        var result = _dut.HasAccess(query);

        // Assert
        Assert.IsFalse(result);
    }
}
