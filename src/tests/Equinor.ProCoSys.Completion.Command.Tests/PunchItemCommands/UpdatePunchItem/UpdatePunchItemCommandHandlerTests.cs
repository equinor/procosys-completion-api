using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
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
    public async Task HandlingCommand_ShouldUpdateRequiredProps_OnPunchItem_WhenOperationsGiven()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(_newDescription, _existingPunchItem.Description);
        Assert.AreEqual(_raisedByOrg.Id, _existingPunchItem.RaisedByOrgId);
        Assert.AreEqual(_clearingByOrg.Id, _existingPunchItem.ClearingByOrgId);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldUpdateOptionalProps_FromNullToValue_OnPunchItem_WhenOperationsGiven()
    {
        // Arrange
        Assert.IsNull(_existingPunchItem.PriorityId);
        Assert.IsNull(_existingPunchItem.SortingId);
        Assert.IsNull(_existingPunchItem.TypeId);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(_priority.Id, _existingPunchItem.PriorityId);
        Assert.AreEqual(_sorting.Id, _existingPunchItem.SortingId);
        Assert.AreEqual(_type.Id, _existingPunchItem.TypeId);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldUpdateOptionalProps_FromValueToNull_OnPunchItem_WhenOperationsGiven()
    {
        // Arrange
        _existingPunchItem.SetPriority(_priority);
        _existingPunchItem.SetSorting(_sorting);
        _existingPunchItem.SetType(_type);

        Assert.IsNotNull(_existingPunchItem.PriorityId);
        Assert.IsNotNull(_existingPunchItem.SortingId);
        Assert.IsNotNull(_existingPunchItem.TypeId);

        _command.PatchDocument.Operations.Clear();
        _command.PatchDocument.Replace(p => p.PriorityGuid, null);
        _command.PatchDocument.Replace(p => p.SortingGuid, null);
        _command.PatchDocument.Replace(p => p.TypeGuid, null);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsNull(_existingPunchItem.PriorityId);
        Assert.IsNull(_existingPunchItem.SortingId);
        Assert.IsNull(_existingPunchItem.TypeId);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldUpdateOptionalProps_FromValueToOtherValue_OnPunchItem_WhenOperationsGiven()
    {
        // Arrange
        _existingPunchItem.SetPriority(_priority);
        _existingPunchItem.SetSorting(_sorting);
        _existingPunchItem.SetType(_type);

        Assert.IsNotNull(_existingPunchItem.PriorityId);
        Assert.IsNotNull(_existingPunchItem.SortingId);
        Assert.IsNotNull(_existingPunchItem.TypeId);

        var priority2 = SetupLibraryItem(Guid.NewGuid(), LibraryType.PUNCHLIST_PRIORITY, _priority.Id+1000);
        var sorting2 = SetupLibraryItem(Guid.NewGuid(), LibraryType.PUNCHLIST_SORTING, _sorting.Id+1000);
        var type2 = SetupLibraryItem(Guid.NewGuid(), LibraryType.PUNCHLIST_TYPE, _type.Id+1000);
        _command.PatchDocument.Operations.Clear();
        _command.PatchDocument.Replace(p => p.PriorityGuid, priority2.Guid);
        _command.PatchDocument.Replace(p => p.SortingGuid, sorting2.Guid);
        _command.PatchDocument.Replace(p => p.TypeGuid, type2.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(priority2.Id, _existingPunchItem.PriorityId);
        Assert.AreEqual(sorting2.Id, _existingPunchItem.SortingId);
        Assert.AreEqual(type2.Id, _existingPunchItem.TypeId);
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
    public async Task HandlingCommand_ShouldSetAndReturnRowVersion_WhenOperationsGiven()
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
        var punchItemUpdatedDomainEventAdded = _existingPunchItem.DomainEvents.Last();
        Assert.IsInstanceOfType(punchItemUpdatedDomainEventAdded, typeof(PunchItemUpdatedDomainEvent));
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldAddChangesToPunchItemUpdatedEvent_WhenOperationsGiven()
    {
        // Arrange
        var oldDescription = _existingPunchItem.Description;
        var oldRaisedByCode = _existingPunchItem.RaisedByOrg.Code;
        var oldClearingByOrg = _existingPunchItem.ClearingByOrg.Code;

        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItemUpdatedDomainEventAdded = _existingPunchItem.DomainEvents.Last() as PunchItemUpdatedDomainEvent;
        Assert.IsNotNull(punchItemUpdatedDomainEventAdded);
        Assert.IsNotNull(punchItemUpdatedDomainEventAdded.Changes);
        Assert.AreEqual(6, _command.PatchDocument.Operations.Count);
        Assert.AreEqual(_command.PatchDocument.Operations.Count, punchItemUpdatedDomainEventAdded.Changes.Count);

        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Description)),
            oldDescription,
            _newDescription);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.RaisedByOrg)),
            oldRaisedByCode,
            _raisedByOrg.Code);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.ClearingByOrg)),
            oldClearingByOrg,
            _clearingByOrg.Code);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Priority)),
            null,
            _priority.Code);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Sorting)),
            null,
            _sorting.Code);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Type)),
            null,
            _type.Code);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldAddChangesToPunchItemUpdatedEvent_WhenOperationsWithNullGiven()
    {
        // Arrange
        _command.PatchDocument.Operations.Clear();
        _command.PatchDocument.Replace(p => p.PriorityGuid, null);
        _command.PatchDocument.Replace(p => p.SortingGuid, null);
        _command.PatchDocument.Replace(p => p.TypeGuid, null);

        _existingPunchItem.SetPriority(_priority);
        _existingPunchItem.SetSorting(_sorting);
        _existingPunchItem.SetType(_type);
        var oldPriorityCode = _priority.Code;
        var oldSortingCode = _sorting.Code;
        var oldTypeCode = _type.Code;

        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItemUpdatedDomainEventAdded = _existingPunchItem.DomainEvents.Last() as PunchItemUpdatedDomainEvent;
        Assert.IsNotNull(punchItemUpdatedDomainEventAdded);
        Assert.IsNotNull(punchItemUpdatedDomainEventAdded.Changes);
        Assert.AreEqual(3, _command.PatchDocument.Operations.Count);
        Assert.AreEqual(_command.PatchDocument.Operations.Count, punchItemUpdatedDomainEventAdded.Changes.Count);

        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Priority)),
            oldPriorityCode,
            null);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Sorting)),
            oldSortingCode,
            null);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Type)),
            oldTypeCode,
            null);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchDescriptionWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _command.PatchDocument.Replace(p => p.Description, _existingPunchItem.Description);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchRaisedByOrgWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _command.PatchDocument.Replace(p => p.RaisedByOrgGuid, _existingPunchItem.RaisedByOrg.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchClearingByOrgWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _command.PatchDocument.Replace(p => p.ClearingByOrgGuid, _existingPunchItem.ClearingByOrg.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchPriorityWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem.SetPriority(_priority);
        _command.PatchDocument.Replace(p => p.PriorityGuid, _existingPunchItem.Priority!.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchSortingWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem.SetSorting(_sorting);
        _command.PatchDocument.Replace(p => p.SortingGuid, _existingPunchItem.Sorting!.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchTypeWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem.SetType(_type);
        _command.PatchDocument.Replace(p => p.TypeGuid, _existingPunchItem.Type!.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }
    #endregion

    #region test update when patchDocument don't contains any operations
    [TestMethod]
    public async Task HandlingCommand_ShouldNotUpdatePunchItem_WhenNoOperationsGiven()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        var oldDescription = _existingPunchItem.Description;
        var oldRaisedByOrgId = _existingPunchItem.RaisedByOrgId;
        var oldClearingByOrgId = _existingPunchItem.ClearingByOrgId;
        var oldPriorityId = _existingPunchItem.PriorityId;
        var oldTypeId = _existingPunchItem.TypeId;
        var oldSortingId = _existingPunchItem.SortingId;
        
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(oldDescription, _existingPunchItem.Description);
        Assert.AreEqual(oldRaisedByOrgId, _existingPunchItem.RaisedByOrgId);
        Assert.AreEqual(oldClearingByOrgId, _existingPunchItem.ClearingByOrgId);
        Assert.AreEqual(oldPriorityId, _existingPunchItem.PriorityId);
        Assert.AreEqual(oldSortingId, _existingPunchItem.SortingId);
        Assert.AreEqual(oldTypeId, _existingPunchItem.TypeId);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSave_WhenNoOperationsGiven()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();

        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync(default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSetAndReturnRowVersion_WhenNoOperationsGiven()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();

        // Act
        var result = await _dut.Handle(_command, default);

        // Assert
        // In real life EF Core will create a new RowVersion when save.
        // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
        Assert.AreEqual(_command.RowVersion, result.Data);
        Assert.AreEqual(_command.RowVersion, _existingPunchItem.RowVersion.ConvertToString());
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
        var str = Guid.NewGuid().ToString();
        var libraryItem = new LibraryItem(
            TestPlantA,
            libraryGuid,
            str.Substring(0, 4),
            str,
            libraryType);
        libraryItem.SetProtectedIdForTesting(id);
        _libraryItemRepositoryMock.GetByGuidAndTypeAsync(libraryGuid, libraryType)
            .Returns(libraryItem);

        return libraryItem;
    }

    private void AssertChange(IProperty change, object oldValue, object newValue)
    {
        Assert.IsNotNull(change);
        Assert.AreEqual(oldValue, change.OldValue);
        Assert.AreEqual(newValue, change.NewValue);
    }
}
