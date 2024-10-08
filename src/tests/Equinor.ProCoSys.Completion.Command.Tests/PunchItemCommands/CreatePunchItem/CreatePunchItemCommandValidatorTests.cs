﻿using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;
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
    private CreatePunchItemCommandValidator<CreatePunchItemCommand> _dut;
    private CreatePunchItemCommand _command;
    private readonly IProjectValidator _projectValidatorMock = Substitute.For<IProjectValidator>();
    private readonly ILibraryItemValidator _libraryItemValidatorMock = Substitute.For<ILibraryItemValidator>();
    private readonly IWorkOrderValidator _workOrderValidatorMock = Substitute.For<IWorkOrderValidator>();
    private readonly ISWCRValidator _swcrValidatorMock = Substitute.For<ISWCRValidator>();
    private readonly IDocumentValidator _documentValidatorMock = Substitute.For<IDocumentValidator>();
    private readonly IPunchItemValidator _punchItemValidatorMock = Substitute.For<IPunchItemValidator>();

    [TestInitialize]
    public void Setup_OkState()
    {
        var checkListGuid = Guid.NewGuid();
        _command = new CreatePunchItemCommand(
            Category.PA,
            "Test desc",
            checkListGuid,
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
            "ExtNo",
            false,
            null,
            null)
        {
            CheckListDetailsDto = new CheckListDetailsDto(checkListGuid, "R", false, Guid.NewGuid())
        };
        _projectValidatorMock.ExistsAsync(_command.CheckListDetailsDto.ProjectGuid, default).Returns(true);

        SetupOkLibraryItem(_command.RaisedByOrgGuid, LibraryType.COMPLETION_ORGANIZATION);
        SetupOkLibraryItem(_command.ClearingByOrgGuid, LibraryType.COMPLETION_ORGANIZATION);
        SetupOkLibraryItem(_command.PriorityGuid!.Value, LibraryType.COMM_PRIORITY);
        SetupOkLibraryItem(_command.SortingGuid!.Value, LibraryType.PUNCHLIST_SORTING);
        SetupOkLibraryItem(_command.TypeGuid!.Value, LibraryType.PUNCHLIST_TYPE);

        _workOrderValidatorMock.ExistsAsync(_command.OriginalWorkOrderGuid!.Value, default).Returns(true);
        _workOrderValidatorMock.ExistsAsync(_command.WorkOrderGuid!.Value, default).Returns(true);
        _swcrValidatorMock.ExistsAsync(_command.SWCRGuid!.Value, default).Returns(true);
        _documentValidatorMock.ExistsAsync(_command.DocumentGuid!.Value, default).Returns(true);

        _dut = new CreatePunchItemCommandValidator<CreatePunchItemCommand>(
            _projectValidatorMock,
            _punchItemValidatorMock,
            _libraryItemValidatorMock,
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
        _projectValidatorMock.ExistsAsync(_command.CheckListDetailsDto.ProjectGuid, default).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project for given check list does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_ProjectIsClosed()
    {
        // Arrange
        _projectValidatorMock.IsClosedAsync(_command.CheckListDetailsDto.ProjectGuid, default).Returns(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project for given check list is closed!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_ExternalItemNoExists()
    {
        // Arrange
        _punchItemValidatorMock.ExternalItemNoExistsInProjectAsync(
            _command.ExternalItemNo!, 
            _command.CheckListDetailsDto.ProjectGuid, default).Returns(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("ExternalItemNo already exists in project!"));
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
                LibraryType.COMM_PRIORITY, 
                default)
            .Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith(
            $"Priority library item is not a {LibraryType.COMM_PRIORITY}!"));
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
    public async Task Validate_ShouldFail_When_TagOwningCheckListIsVoided()
    {
        // Arrange
        _command.CheckListDetailsDto = new CheckListDetailsDto(_command.CheckListGuid, "R", true, _command.CheckListDetailsDto.ProjectGuid);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Tag owning check list is voided!"));
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
    public async Task Validate_ShouldFail_When_OriginalWorkOrderIsClosed()
    {
        // Arrange
        _workOrderValidatorMock.IsVoidedAsync(_command.OriginalWorkOrderGuid!.Value, default).Returns(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Original WO is closed!"));
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
    public async Task Validate_ShouldFail_When_WorkOrderIsClosed()
    {
        // Arrange
        _workOrderValidatorMock.IsVoidedAsync(_command.WorkOrderGuid!.Value, default).Returns(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("WO is closed!"));
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
