using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UpdatePunchItem;

[TestClass]
public class UpdatePunchItemCommandHandlerTests : PunchItemCommandHandlerTestsBase
{
    private UpdatePunchItemCommand _command;
    private UpdatePunchItemCommandHandler _dut;
    private readonly string _newDescription = "new description";
    protected ILibraryItemRepository _libraryItemRepositoryMock;
    private readonly Guid _raisedByOrgGuid = Guid.NewGuid();
    private readonly Guid _clearingByOrgGuid = Guid.NewGuid();
    private readonly Guid _priorityGuid = Guid.NewGuid();
    private readonly Guid _sortingGuid = Guid.NewGuid();
    private readonly Guid _typeGuid = Guid.NewGuid();
    private LibraryItem _raisedByOrg;
    private LibraryItem _clearingByOrg;
    private LibraryItem _priority;
    private LibraryItem _sorting;
    private LibraryItem _type;

    [TestInitialize]
    public void Setup()
    {
        _command = new UpdatePunchItemCommand(
            _existingPunchItem.Guid,
            new JsonPatchDocument<PatchablePunchItem>(),
            RowVersion);
        _command.PatchDocument.Replace(p => p.Description, _newDescription);
        _command.PatchDocument.Replace(p => p.RaisedByOrgGuid, _raisedByOrgGuid);
        _command.PatchDocument.Replace(p => p.ClearingByOrgGuid, _clearingByOrgGuid);
        _command.PatchDocument.Replace(p => p.PriorityGuid, _priorityGuid);
        _command.PatchDocument.Replace(p => p.SortingGuid, _sortingGuid);
        _command.PatchDocument.Replace(p => p.TypeGuid, _typeGuid);

        _command.EnsureValidInputValidation();

        _libraryItemRepositoryMock = Substitute.For<ILibraryItemRepository>();

        _raisedByOrg = SetupLibraryItem(_raisedByOrgGuid, LibraryType.COMPLETION_ORGANIZATION, 100);
        _clearingByOrg = SetupLibraryItem(_clearingByOrgGuid, LibraryType.COMPLETION_ORGANIZATION, 110);
        _priority = SetupLibraryItem(_priorityGuid, LibraryType.PUNCHLIST_PRIORITY, 120);
        _sorting = SetupLibraryItem(_sortingGuid, LibraryType.PUNCHLIST_SORTING, 130);
        _type = SetupLibraryItem(_typeGuid, LibraryType.PUNCHLIST_TYPE, 140);

        _dut = new UpdatePunchItemCommandHandler(
            _punchItemRepositoryMock,
            _libraryItemRepositoryMock,
            _unitOfWorkMock,
            Substitute.For<ILogger<UpdatePunchItemCommandHandler>>());
    }

    #region test update when patchDocument contains operations
    [TestMethod]
    public async Task HandlingCommand_ShouldUpdatePunchItem_WhenOperationsGiven()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(_newDescription, _existingPunchItem.Description);
        Assert.AreEqual(_raisedByOrg.Id, _existingPunchItem.RaisedByOrgId);
        Assert.AreEqual(_clearingByOrg.Id, _existingPunchItem.ClearingByOrgId);
        Assert.AreEqual(_priority.Id, _existingPunchItem.PriorityId);
        Assert.AreEqual(_sorting.Id, _existingPunchItem.SortingId);
        Assert.AreEqual(_type.Id, _existingPunchItem.TypeId);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldUpdatePunchItem_WhenOperationsWithNullGiven()
    {
        // Arrange
        _command.PatchDocument.Operations.Clear();
        _command.PatchDocument.Replace(p => p.PriorityGuid, null);
        _command.PatchDocument.Replace(p => p.SortingGuid, null);
        _command.PatchDocument.Replace(p => p.TypeGuid, null);

        _existingPunchItem.SetPriority(_priority);
        _existingPunchItem.SetSorting(_sorting);
        _existingPunchItem.SetType(_type);
        Assert.AreEqual(_priority.Id, _existingPunchItem.PriorityId);
        Assert.AreEqual(_sorting.Id, _existingPunchItem.SortingId);
        Assert.AreEqual(_type.Id, _existingPunchItem.TypeId);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsNull(_existingPunchItem.PriorityId);
        Assert.IsNull(_existingPunchItem.SortingId);
        Assert.IsNull(_existingPunchItem.TypeId);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSave_WhenOperationsGiven()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync(default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSetAndReturnNewRowVersion_WhenOperationsGiven()
    {
        // Act
        var result = await _dut.Handle(_command, default);

        // Assert
        // In real life EF Core will create a new RowVersion when save.
        // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
        Assert.AreEqual(_command.RowVersion, result.Data);
        Assert.AreEqual(_command.RowVersion, _existingPunchItem.RowVersion.ConvertToString());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldAddPunchItemUpdatedEvent_WhenOperationsGiven()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsInstanceOfType(_existingPunchItem.DomainEvents.Last(), typeof(PunchItemUpdatedDomainEvent));
    }
    #endregion

    #region test update when patchDocument don't contains any operations
    [TestMethod]
    public async Task HandlingCommand_ShouldNotUpdatePunchItem_WhenNoOperationsGiven()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        var oldDescription = _existingPunchItem.Description;
        
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(oldDescription, _existingPunchItem.Description);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotSave_WhenNoOperationsGiven()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();

        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(0).SaveChangesAsync(default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldReturnOldRowVersion_WhenNoOperationsGiven()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        var oldRowVersion = _existingPunchItem.RowVersion.ConvertToString();

        // Act
        var result = await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(oldRowVersion, result.Data);
        Assert.AreEqual(oldRowVersion, _existingPunchItem.RowVersion.ConvertToString());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenNoOperationsGiven()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();

        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }
    #endregion

    private LibraryItem SetupLibraryItem(Guid libraryGuid, LibraryType libraryType, int id)
    {
        var libraryItem = new LibraryItem(TestPlantA, libraryGuid, null!, null!, libraryType);
        libraryItem.SetProtectedIdForTesting(id);
        _libraryItemRepositoryMock.GetByGuidAndTypeAsync(libraryGuid, libraryType)
            .Returns(libraryItem);

        return libraryItem;
    }
}
