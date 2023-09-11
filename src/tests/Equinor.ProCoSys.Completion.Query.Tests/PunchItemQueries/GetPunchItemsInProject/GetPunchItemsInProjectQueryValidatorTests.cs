using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsInProject;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Query.Tests.PunchItemQueries.GetPunchItemsInProject;

[TestClass]
public class GetPunchItemsInProjectQueryValidatorTests
{
    private GetPunchItemsInProjectQueryValidator _dut;
    private GetPunchItemsInProjectQuery _query;
    private IProjectValidator _projectValidatorMock;

    [TestInitialize]
    public void Setup_OkState()
    {
        _query = new GetPunchItemsInProjectQuery(Guid.NewGuid());
        _projectValidatorMock = Substitute.For<IProjectValidator>();
        _projectValidatorMock.ExistsAsync(_query.ProjectGuid, default).Returns(true);

        _dut = new GetPunchItemsInProjectQueryValidator(_projectValidatorMock);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        // Act
        var result = await _dut.ValidateAsync(_query);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_ProjectNotExists()
    {
        // Arrange
        _projectValidatorMock.ExistsAsync(_query.ProjectGuid, default).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_query);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        var validationFailure = result.Errors[0];
        Assert.IsTrue(validationFailure.ErrorMessage.StartsWith("Project with this guid does not exist!"));
        Assert.IsInstanceOfType(validationFailure.CustomState, typeof(EntityNotFoundException));
    }
}
