using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsProjectCommandTests;

[TestClass]
public abstract class AccessValidatorForIIsProjectCommandTests<TProjectRequest> : AccessValidatorTestBase
    where TProjectRequest : IBaseRequest, IIsProjectCommand
{
    protected abstract TProjectRequest GetProjectRequestWithAccessToProjectToTest();
    protected abstract TProjectRequest GetProjectRequestWithoutAccessToProjectToTest();

    [TestMethod]
    public async Task Validate_ShouldReturnTrue_WhenAccessToProject()
    {
        // Arrange
        var command = GetProjectRequestWithAccessToProjectToTest();

        // act
        var result = await _dut.ValidateAsync(command);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Validate_ShouldReturnFalse_WhenNoAccessToProject()
    {
        // Arrange
        var command = GetProjectRequestWithoutAccessToProjectToTest();

        // act
        var result = await _dut.ValidateAsync(command);

        // Assert
        Assert.IsFalse(result);
    }
}
