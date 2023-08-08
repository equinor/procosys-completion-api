using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.Command.Validators.LibraryItemValidators;
using Equinor.ProCoSys.Completion.Command.Validators.ProjectValidators;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.CreatePunchItem;

[TestClass]
public class CreatePunchItemCommandValidatorTests
{
    private CreatePunchItemCommandValidator _dut;
    private CreatePunchItemCommand _command;
    private Mock<IProjectValidator> _projectValidatorMock;
    private Mock<ILibraryItemValidator> _libraryItemValidatorMock;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new CreatePunchItemCommand(
            "Test title",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());
        _projectValidatorMock = new Mock<IProjectValidator>();
        _projectValidatorMock.Setup(x => x.ExistsAsync(_command.ProjectGuid, default))
            .ReturnsAsync(true);
        _libraryItemValidatorMock = new Mock<ILibraryItemValidator>();

        SetupOkLibraryItem(_command.RaisedByOrgGuid, LibraryType.COMPLETION_ORGANIZATION);
        SetupOkLibraryItem(_command.ClearingByOrgGuid, LibraryType.COMPLETION_ORGANIZATION);
        SetupOkLibraryItem(_command.PriorityGuid!.Value, LibraryType.PUNCHLIST_PRIORITY);
        SetupOkLibraryItem(_command.SortingGuid!.Value, LibraryType.PUNCHLIST_SORTING);
        SetupOkLibraryItem(_command.TypeGuid!.Value, LibraryType.PUNCHLIST_TYPE);

        _dut = new CreatePunchItemCommandValidator(
            _projectValidatorMock.Object,
            _libraryItemValidatorMock.Object);
    }

    private void SetupOkLibraryItem(Guid guid, LibraryType libraryType)
    {
        _libraryItemValidatorMock.Setup(x => x.ExistsAsync(guid, default))
            .ReturnsAsync(true);
        _libraryItemValidatorMock.Setup(x => x.HasTypeAsync(
                guid,
                libraryType,
                default))
            .ReturnsAsync(true);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_ProjectNotExists()
    {
        // Arrange
        _projectValidatorMock.Setup(x => x.ExistsAsync(_command.ProjectGuid, default))
            .ReturnsAsync(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_ProjectIsClosed()
    {
        // Arrange
        _projectValidatorMock.Setup(x => x.IsClosedAsync(_command.ProjectGuid, default))
            .ReturnsAsync(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project is closed!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_RaisedByOrgNotExists()
    {
        // Arrange
        _libraryItemValidatorMock.Setup(x => x.ExistsAsync(_command.RaisedByOrgGuid, default))
            .ReturnsAsync(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("RaisedByOrg library item does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_RaisedByOrgIsVoided()
    {
        // Arrange
        _libraryItemValidatorMock.Setup(x => x.IsVoidedAsync(_command.RaisedByOrgGuid, default))
            .ReturnsAsync(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("RaisedByOrg library item is voided!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_RaisedByOrgIsNotACompletionOrganization()
    {
        // Arrange
        _libraryItemValidatorMock.Setup(x => x.HasTypeAsync(_command.RaisedByOrgGuid,
                LibraryType.COMPLETION_ORGANIZATION,
                default))
            .ReturnsAsync(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith(
            $"RaisedByOrg library item is not a {LibraryType.COMPLETION_ORGANIZATION}!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_ClearingByOrgGuidNotExists()
    {
        // Arrange
        _libraryItemValidatorMock.Setup(x => x.ExistsAsync(_command.ClearingByOrgGuid, default))
            .ReturnsAsync(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("ClearingByOrg library item does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_ClearingByOrgGuidIsVoided()
    {
        // Arrange
        _libraryItemValidatorMock.Setup(x => x.IsVoidedAsync(_command.ClearingByOrgGuid, default))
            .ReturnsAsync(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("ClearingByOrg library item is voided!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_ClearingByOrgIsNotACompletionOrganization()
    {
        // Arrange
        _libraryItemValidatorMock.Setup(x => x.HasTypeAsync(_command.ClearingByOrgGuid,
                LibraryType.COMPLETION_ORGANIZATION,
                default))
            .ReturnsAsync(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith(
            $"ClearingByOrg library item is not a {LibraryType.COMPLETION_ORGANIZATION}!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_PriorityGuidNotExists()
    {
        // Arrange
        _libraryItemValidatorMock.Setup(x => x.ExistsAsync(_command.PriorityGuid!.Value, default))
            .ReturnsAsync(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Priority library item does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_PriorityGuidIsVoided()
    {
        // Arrange
        _libraryItemValidatorMock.Setup(x => x.IsVoidedAsync(_command.PriorityGuid!.Value, default))
            .ReturnsAsync(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Priority library item is voided!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_PriorityIsNotAPriority()
    {
        // Arrange
        _libraryItemValidatorMock.Setup(x => x.HasTypeAsync(_command.PriorityGuid!.Value,
                LibraryType.PUNCHLIST_PRIORITY,
                default))
            .ReturnsAsync(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith(
            $"Priority library item is not a {LibraryType.PUNCHLIST_PRIORITY}!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_SortingGuidNotExists()
    {
        // Arrange
        _libraryItemValidatorMock.Setup(x => x.ExistsAsync(_command.SortingGuid!.Value, default))
            .ReturnsAsync(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Sorting library item does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_SortingGuidIsVoided()
    {
        // Arrange
        _libraryItemValidatorMock.Setup(x => x.IsVoidedAsync(_command.SortingGuid!.Value, default))
            .ReturnsAsync(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Sorting library item is voided!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_SortingIsNotASorting()
    {
        // Arrange
        _libraryItemValidatorMock.Setup(x => x.HasTypeAsync(_command.SortingGuid!.Value,
                LibraryType.PUNCHLIST_SORTING,
                default))
            .ReturnsAsync(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith(
            $"Sorting library item is not a {LibraryType.PUNCHLIST_SORTING}!"));
    }
}
