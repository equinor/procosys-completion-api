using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Equinor.ProCoSys.Completion.Query.CheckListQueries.GetPunchItems;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Query.Tests.CheckListQueries.GetPunchItems;

[TestClass]
public class GetPunchItemsQueryValidatorTests
{
    private GetPunchItemsQueryValidator _dut;
    private GetPunchItemsQuery _query;
    private readonly Guid _projectGuid = Guid.NewGuid();
    private readonly IProjectValidator _projectValidatorMock = Substitute.For<IProjectValidator>();

    [TestInitialize]
    public void Setup_OkState()
    {
        _query = new GetPunchItemsQuery(_projectGuid)
        {
            CheckListDetailsDto = new CheckListDetailsDto(Guid.Empty, "FT", "FG", "RC", "TRC", "TRD", "TFC", "TFD", _projectGuid)
        };

        _projectValidatorMock.ExistsAsync(_query.CheckListDetailsDto.ProjectGuid, default).Returns(true);
        _dut = new GetPunchItemsQueryValidator(_projectValidatorMock);
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
        _projectValidatorMock.ExistsAsync(_query.CheckListDetailsDto.ProjectGuid, default).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_query);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project with this guid does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WithCustomState_When_ProjectNotExists()
    {
        // Arrange
        _projectValidatorMock.ExistsAsync(_query.CheckListDetailsDto.ProjectGuid, default).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_query);

        // Assert
        var error = result.Errors[0];
        Assert.IsTrue(error.CustomState is EntityNotFoundException);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_ProjectIsClosed()
    {
        // Arrange
        _projectValidatorMock.IsClosedAsync(_query.CheckListDetailsDto.ProjectGuid, default).Returns(true);

        // Act
        var result = await _dut.ValidateAsync(_query);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project is closed. Punch items are not available in closed projects!"));
    }
}
