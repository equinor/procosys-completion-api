using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsCheckListQueryTests;

[TestClass]
public abstract class AccessValidatorForIIsCheckListQueryTests<TCheckListQuery> : AccessValidatorTestBase
    where TCheckListQuery : IBaseRequest, IIsCheckListQuery
{
    protected abstract TCheckListQuery GetCheckListQueryWithAccessToProject();
    protected abstract TCheckListQuery GetCheckListQueryWithoutAccessToProject();

    [TestMethod]
    public async Task Validate_ShouldReturnTrue_WhenAccessToProjectForCheckList()
    {
        // Arrange
        var query = GetCheckListQueryWithAccessToProject();

        // act
        var result = await _dut.ValidateAsync(query, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Validate_ShouldReturnFalse_WhenNoAccessToProjectForCheckList()
    {
        // Arrange
        var query = GetCheckListQueryWithoutAccessToProject();

        // act
        var result = await _dut.ValidateAsync(query, default);

        // Assert
        Assert.IsFalse(result);
    }
}
