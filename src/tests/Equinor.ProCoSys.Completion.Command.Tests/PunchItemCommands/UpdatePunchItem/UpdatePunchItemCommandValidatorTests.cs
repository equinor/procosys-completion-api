using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
 using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UpdatePunchItem;

[TestClass]
public class UpdatePunchItemCommandValidatorTests
{
    private UpdatePunchItemCommandValidator _dut;
    private IPunchItemValidator _punchItemValidatorMock;
    private ILibraryItemValidator _libraryItemValidatorMock;
    private UpdatePunchItemCommand _command;
    private readonly JsonPatchDocument<PatchablePunchItem> _jsonPatchDocument = new();
    private readonly Guid _raisedByOrgGuid = Guid.NewGuid();
    private readonly Guid _clearingByOrgGuid = Guid.NewGuid();
    private readonly Guid _priorityGuid = Guid.NewGuid();
    private readonly Guid _sortingGuid = Guid.NewGuid();
    private readonly Guid _typeGuid = Guid.NewGuid();

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new UpdatePunchItemCommand(Guid.NewGuid(), _jsonPatchDocument, "r");
        _command.PatchDocument.Replace(p => p.RaisedByOrgGuid, _raisedByOrgGuid);
        _command.PatchDocument.Replace(p => p.ClearingByOrgGuid, _clearingByOrgGuid);
        _command.PatchDocument.Replace(p => p.PriorityGuid, _priorityGuid);
        _command.PatchDocument.Replace(p => p.SortingGuid, _sortingGuid);
        _command.PatchDocument.Replace(p => p.TypeGuid, _typeGuid);

        _punchItemValidatorMock = Substitute.For<IPunchItemValidator>();
        _punchItemValidatorMock.ExistsAsync(_command.PunchItemGuid, default)
            .Returns(true);
        _libraryItemValidatorMock = Substitute.For<ILibraryItemValidator>();

        SetupOkLibraryItem(_raisedByOrgGuid, LibraryType.COMPLETION_ORGANIZATION);
        SetupOkLibraryItem(_clearingByOrgGuid, LibraryType.COMPLETION_ORGANIZATION);
        SetupOkLibraryItem(_priorityGuid, LibraryType.PUNCHLIST_PRIORITY);
        SetupOkLibraryItem(_sortingGuid, LibraryType.PUNCHLIST_SORTING);
        SetupOkLibraryItem(_typeGuid, LibraryType.PUNCHLIST_TYPE);

        _dut = new UpdatePunchItemCommandValidator(_punchItemValidatorMock, _libraryItemValidatorMock);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState_WithPatchingOfGuids()
    {
        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState_WithPatchingOfGuidsAsString()
    {
        // Arrange
        _command.PatchDocument.Operations.Clear();
        _command.PatchDocument.Operations.Add(new Operation<PatchablePunchItem>(
            "replace",
            $"/{nameof(PatchablePunchItem.RaisedByOrgGuid)}",
            _raisedByOrgGuid.ToString()));
        _command.PatchDocument.Operations.Add(new Operation<PatchablePunchItem>(
            "replace",
            $"/{nameof(PatchablePunchItem.ClearingByOrgGuid)}",
            _clearingByOrgGuid.ToString()));
        _command.PatchDocument.Operations.Add(new Operation<PatchablePunchItem>(
            "replace",
            $"/{nameof(PatchablePunchItem.PriorityGuid)}",
            _priorityGuid.ToString()));
        _command.PatchDocument.Operations.Add(new Operation<PatchablePunchItem>(
            "replace",
            $"/{nameof(PatchablePunchItem.SortingGuid)}",
            _sortingGuid.ToString()));
        _command.PatchDocument.Operations.Add(new Operation<PatchablePunchItem>(
            "replace",
            $"/{nameof(PatchablePunchItem.TypeGuid)}",
            _typeGuid.ToString()));

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState_WithPatchingOfGuidsAsNull()
    {
        // Arrange
        _command.PatchDocument.Operations.Clear();
        _command.PatchDocument.Replace(p => p.PriorityGuid, null);
        _command.PatchDocument.Replace(p => p.SortingGuid, null);
        _command.PatchDocument.Replace(p => p.TypeGuid, null);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState_WithOutPatching()
    {
        // Arrange
        _command.PatchDocument.Operations.Clear();

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_PunchItemNotExists()
    {
        // Arrange
        _punchItemValidatorMock.ExistsAsync(_command.PunchItemGuid, default)
            .Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch item with this guid does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_TagOwningPunchItemIsVoided()
    {
        // Arrange
        _punchItemValidatorMock.TagOwningPunchItemIsVoidedAsync(_command.PunchItemGuid, default)
            .Returns(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Tag owning punch item is voided!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_ProjectIsClosed()
    {
        // Arrange
        _punchItemValidatorMock.ProjectOwningPunchItemIsClosedAsync(_command.PunchItemGuid, default)
            .Returns(true);

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
        _libraryItemValidatorMock.ExistsAsync(_raisedByOrgGuid, default).Returns(false);

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
        _libraryItemValidatorMock.IsVoidedAsync(_raisedByOrgGuid, default).Returns(true);

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
        _libraryItemValidatorMock.HasTypeAsync(_raisedByOrgGuid, LibraryType.COMPLETION_ORGANIZATION, default).Returns(false);

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
        _libraryItemValidatorMock.ExistsAsync(_clearingByOrgGuid, default).Returns(false);

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
        _libraryItemValidatorMock.IsVoidedAsync(_clearingByOrgGuid, default).Returns(true);

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
        _libraryItemValidatorMock.HasTypeAsync(_clearingByOrgGuid, LibraryType.COMPLETION_ORGANIZATION, default)
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
        _libraryItemValidatorMock.ExistsAsync(_priorityGuid, default).Returns(false);

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
        _libraryItemValidatorMock.IsVoidedAsync(_priorityGuid, default).Returns(true);

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
                _priorityGuid,
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
        _libraryItemValidatorMock.ExistsAsync(_sortingGuid, default).Returns(false);

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
        _libraryItemValidatorMock.IsVoidedAsync(_sortingGuid, default).Returns(true);

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
                _sortingGuid,
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
        _libraryItemValidatorMock.ExistsAsync(_typeGuid, default).Returns(false);

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
        _libraryItemValidatorMock.IsVoidedAsync(_typeGuid, default).Returns(true);

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
                _typeGuid,
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
    private void SetupOkLibraryItem(Guid guid, LibraryType libraryType)
    {
        _libraryItemValidatorMock.ExistsAsync(guid, default).Returns(true);
        _libraryItemValidatorMock.HasTypeAsync(guid, libraryType, default).Returns(true);
    }
}
