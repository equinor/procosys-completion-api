using Equinor.ProCoSys.Completion.Domain;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations;

[TestClass]
public abstract class AccessValidatorForQueryNeedAccessTests<TQuery> : AccessValidatorTestBase
    where TQuery : INeedProjectAccess, IBaseRequest
{
    protected abstract TQuery GetQueryWithAccessToProjectToTest();
    protected abstract TQuery GetQueryWithoutAccessToProjectToTest();

    [TestMethod]
    public void Validate_ShouldReturnTrue_WhenAccessToProject()
    {
        // Arrange
        var query = GetQueryWithAccessToProjectToTest();

        // act
        var result = _dut.HasAccess(query);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Validate_ShouldReturnFalse_WhenNoAccessToProject()
    {
        // Arrange
        var query = GetQueryWithoutAccessToProjectToTest();

        // act
        var result = _dut.HasAccess(query);

        // Assert
        Assert.IsFalse(result);
    }
}
