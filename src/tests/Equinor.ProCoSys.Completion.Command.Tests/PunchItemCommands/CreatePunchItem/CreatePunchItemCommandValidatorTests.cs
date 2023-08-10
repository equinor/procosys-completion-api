using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.Command.Validators.LibraryItemValidators;
using Equinor.ProCoSys.Completion.Command.Validators.ProjectValidators;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
 using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.CreatePunchItem;

[TestClass]
public class CreatePunchItemCommandValidatorTests
{
    private CreatePunchItemCommandValidator _dut;
    private CreatePunchItemCommand _command;
    private IProjectValidator _projectValidatorMock;
    private ILibraryItemValidator _libraryItemValidatorMock;

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
        _projectValidatorMock = Substitute.For<IProjectValidator>();
        _projectValidatorMock.ExistsAsync(_command.ProjectGuid, default).Returns(true);
        _libraryItemValidatorMock = Substitute.For<ILibraryItemValidator>();

        SetupOkLibraryItem(_command.RaisedByOrgGuid, LibraryType.COMPLETION_ORGANIZATION);
        SetupOkLibraryItem(_command.ClearingByOrgGuid, LibraryType.COMPLETION_ORGANIZATION);
        SetupOkLibraryItem(_command.PriorityGuid!.Value, LibraryType.PUNCHLIST_PRIORITY);
        SetupOkLibraryItem(_command.SortingGuid!.Value, LibraryType.PUNCHLIST_SORTING);
        SetupOkLibraryItem(_command.TypeGuid!.Value, LibraryType.PUNCHLIST_TYPE);

        _dut = new CreatePunchItemCommandValidator(
            _projectValidatorMock,
            _libraryItemValidatorMock);
    }

    private void SetupOkLibraryItem(Guid guid, LibraryType libraryType)
    {
        _libraryItemValidatorMock.ExistsAsync(guid, default).Returns(true);
        _libraryItemValidatorMock.HasTypeAsync(guid, libraryType, default).Returns(true);
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
        _projectValidatorMock.ExistsAsync(_command.ProjectGuid, default).Returns(false);

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
        _projectValidatorMock.IsClosedAsync(_command.ProjectGuid, default).Returns(true);

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
        _libraryItemValidatorMock.ExistsAsync(_command.RaisedByOrgGuid, default).Returns(false);

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
        _libraryItemValidatorMock.IsVoidedAsync(_command.RaisedByOrgGuid, default).Returns(true);

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
        _libraryItemValidatorMock.HasTypeAsync(_command.RaisedByOrgGuid, LibraryType.COMPLETION_ORGANIZATION, default).Returns(false);

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
        _libraryItemValidatorMock.ExistsAsync(_command.ClearingByOrgGuid, default).Returns(false);

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
        _libraryItemValidatorMock.IsVoidedAsync(_command.ClearingByOrgGuid, default).Returns(true);

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
        _libraryItemValidatorMock.HasTypeAsync(_command.ClearingByOrgGuid, LibraryType.COMPLETION_ORGANIZATION, default)
            .Returns(false);

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
        _libraryItemValidatorMock.ExistsAsync(_command.PriorityGuid!.Value, default).Returns(false);

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
        _libraryItemValidatorMock.IsVoidedAsync(_command.PriorityGuid!.Value, default).Returns(true);

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
        _libraryItemValidatorMock.HasTypeAsync(
                _command.PriorityGuid!.Value,
                LibraryType.PUNCHLIST_PRIORITY, 
                default)
            .Returns(false);

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
        _libraryItemValidatorMock.ExistsAsync(_command.SortingGuid!.Value, default).Returns(false);

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
        _libraryItemValidatorMock.IsVoidedAsync(_command.SortingGuid!.Value, default).Returns(true);

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
        _libraryItemValidatorMock.HasTypeAsync(
                _command.SortingGuid!.Value, 
                LibraryType.PUNCHLIST_SORTING, 
                default)
            .Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith(
            $"Sorting library item is not a {LibraryType.PUNCHLIST_SORTING}!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_TypeGuidNotExists()
    {
        // Arrange
        _libraryItemValidatorMock.ExistsAsync(_command.TypeGuid!.Value, default).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Type library item does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_TypeGuidIsVoided()
    {
        // Arrange
        _libraryItemValidatorMock.IsVoidedAsync(_command.TypeGuid!.Value, default).Returns(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Type library item is voided!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_TypeIsNotAType()
    {
        // Arrange
        _libraryItemValidatorMock.HasTypeAsync(
                _command.TypeGuid!.Value,
                LibraryType.PUNCHLIST_TYPE, 
                default)
            .Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith(
            $"Type library item is not a {LibraryType.PUNCHLIST_TYPE}!"));
    }
}
