using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.CreatePunchItem;

[TestClass]
public class CreatePunchItemCommandValidatorTests
{
    private CreatePunchItemCommandValidator _dut;
    private CreatePunchItemCommand _command;
    private IProjectValidator _projectValidatorMock;
    private ICheckListValidator _checkListValidatorMock;
    private ILibraryItemValidator _libraryItemValidatorMock;
    private IPersonValidator _personValidatorMock;
    private IWorkOrderValidator _workOrderValidatorMock;
    private ISWCRValidator _swcrValidatorMock;
    private IDocumentValidator _documentValidatorMock;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new CreatePunchItemCommand(
            Category.PA,
            "Test desc",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            false,
            null,
            null);
        _projectValidatorMock = Substitute.For<IProjectValidator>();
        _projectValidatorMock.ExistsAsync(_command.ProjectGuid, default).Returns(true);
        _checkListValidatorMock = Substitute.For<ICheckListValidator>();
        _checkListValidatorMock.ExistsAsync(_command.CheckListGuid).Returns(true);
        _checkListValidatorMock.InProjectAsync(_command.CheckListGuid, _command.ProjectGuid).Returns(true);

        _libraryItemValidatorMock = Substitute.For<ILibraryItemValidator>();

        SetupOkLibraryItem(_command.RaisedByOrgGuid, LibraryType.COMPLETION_ORGANIZATION);
        SetupOkLibraryItem(_command.ClearingByOrgGuid, LibraryType.COMPLETION_ORGANIZATION);
        SetupOkLibraryItem(_command.PriorityGuid!.Value, LibraryType.PUNCHLIST_PRIORITY);
        SetupOkLibraryItem(_command.SortingGuid!.Value, LibraryType.PUNCHLIST_SORTING);
        SetupOkLibraryItem(_command.TypeGuid!.Value, LibraryType.PUNCHLIST_TYPE);

        _personValidatorMock = Substitute.For<IPersonValidator>();
        _personValidatorMock.ExistsAsync(_command.ActionByPersonOid!.Value, default).Returns(true);
        _workOrderValidatorMock = Substitute.For<IWorkOrderValidator>();
        _workOrderValidatorMock.ExistsAsync(_command.OriginalWorkOrderGuid!.Value, default).Returns(true);
        _workOrderValidatorMock.ExistsAsync(_command.WorkOrderGuid!.Value, default).Returns(true);
        _swcrValidatorMock = Substitute.For<ISWCRValidator>();
        _swcrValidatorMock.ExistsAsync(_command.SWCRGuid!.Value, default).Returns(true);
        _documentValidatorMock = Substitute.For<IDocumentValidator>();
        _documentValidatorMock.ExistsAsync(_command.DocumentGuid!.Value, default).Returns(true);

        _dut = new CreatePunchItemCommandValidator(
            _projectValidatorMock,
            _checkListValidatorMock,
            _libraryItemValidatorMock,
            _personValidatorMock,
            _workOrderValidatorMock,
            _swcrValidatorMock,
            _documentValidatorMock);
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
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project with this guid does not exist!"));
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

    [TestMethod]
    public async Task Validate_ShouldFail_When_CheckListNotExists()
    {
        // Arrange
        _checkListValidatorMock.ExistsAsync(_command.CheckListGuid).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Check list does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_TagOwningCheckListIsVoided()
    {
        // Arrange
        _checkListValidatorMock.TagOwningCheckListIsVoidedAsync(_command.CheckListGuid).Returns(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Tag owning check list is voided!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_CheckListNotInGivenProject()
    {
        // Arrange
        _checkListValidatorMock.InProjectAsync(_command.CheckListGuid, _command.ProjectGuid).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Check list is not in given project!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_ActionByPersonNotExists()
    {
        // Arrange
        _personValidatorMock.ExistsAsync(_command.ActionByPersonOid!.Value, default).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Action by person does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_OriginalWorkOrderNotExists()
    {
        // Arrange
        _workOrderValidatorMock.ExistsAsync(_command.OriginalWorkOrderGuid!.Value, default).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Original WO does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_WorkOrderNotExists()
    {
        // Arrange
        _workOrderValidatorMock.ExistsAsync(_command.WorkOrderGuid!.Value, default).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("WO does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_SWCRNotExists()
    {
        // Arrange
        _swcrValidatorMock.ExistsAsync(_command.SWCRGuid!.Value, default).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("SWCR does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_SWCRIsVoided()
    {
        // Arrange
        _swcrValidatorMock.IsVoidedAsync(_command.SWCRGuid!.Value, default).Returns(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("SWCR is voided!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_DocumentNotExists()
    {
        // Arrange
        _documentValidatorMock.ExistsAsync(_command.DocumentGuid!.Value, default).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Document does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_DocumentIsVoided()
    {
        // Arrange
        _documentValidatorMock.IsVoidedAsync(_command.DocumentGuid!.Value, default).Returns(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Document is voided!"));
    }
}
