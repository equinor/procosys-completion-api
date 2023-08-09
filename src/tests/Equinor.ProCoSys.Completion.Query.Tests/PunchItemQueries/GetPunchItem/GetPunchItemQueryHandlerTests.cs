using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItem;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.Tests.PunchItemQueries.GetPunchItem;

[TestClass]
public class GetPunchItemQueryHandlerTests : ReadOnlyTestsBase
{
    private PunchItem _createdPunchItem;
    private PunchItem _modifiedPunchItem;
    private PunchItem _clearedPunchItem;
    private PunchItem _rejectedPunchItem;
    private PunchItem _verifiedPunchItem;
    private PunchItem _punchItemWithPriority;
    private PunchItem _punchItemWithSorting;
    private PunchItem _punchItemWithType;
    private PunchItem _punchItemWithoutPriority;
    private PunchItem _punchItemWithoutSorting;
    private PunchItem _punchItemWithoutType;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        _createdPunchItem = new PunchItem(TestPlantA, _projectA, "Desc", _raisedByOrg, _clearingByOrg);
        _createdPunchItem.SetPriority(_priority);
        _createdPunchItem.SetSorting(_sorting);
        _createdPunchItem.SetType(_type);
        _modifiedPunchItem = new PunchItem(TestPlantA, _projectA, "Desc", _raisedByOrg, _clearingByOrg);
        _clearedPunchItem = new PunchItem(TestPlantA, _projectA, "Desc", _raisedByOrg, _clearingByOrg);
        _verifiedPunchItem = new PunchItem(TestPlantA, _projectA, "Desc", _raisedByOrg, _clearingByOrg);
        _rejectedPunchItem = new PunchItem(TestPlantA, _projectA, "Desc", _raisedByOrg, _clearingByOrg);

        _punchItemWithPriority = _punchItemWithSorting = _punchItemWithType = _createdPunchItem;
        _punchItemWithoutPriority = _punchItemWithoutSorting = _punchItemWithoutType = _modifiedPunchItem;

        context.PunchItems.Add(_createdPunchItem);
        context.PunchItems.Add(_modifiedPunchItem);
        context.PunchItems.Add(_clearedPunchItem);
        context.PunchItems.Add(_verifiedPunchItem);
        context.PunchItems.Add(_rejectedPunchItem);
        context.SaveChangesAsync().Wait();

        // Elapse some time between each update and save to be able to that 
        // timestamps of Created, Modified, Cleared and Verified differ
        _timeProvider.Elapse(new TimeSpan(0, 1, 0));
        _modifiedPunchItem.Update("Modified");
        context.SaveChangesAsync().Wait();

        _timeProvider.Elapse(new TimeSpan(0, 1, 0));
        _clearedPunchItem.Clear(_currentPerson);
        context.SaveChangesAsync().Wait();

        _timeProvider.Elapse(new TimeSpan(0, 1, 0));
        _verifiedPunchItem.Clear(_currentPerson);
        context.SaveChangesAsync().Wait();

        _timeProvider.Elapse(new TimeSpan(0, 1, 0));
        _verifiedPunchItem.Verify(_currentPerson);
        context.SaveChangesAsync().Wait();

        _timeProvider.Elapse(new TimeSpan(0, 1, 0));
        _rejectedPunchItem.Clear(_currentPerson);
        context.SaveChangesAsync().Wait();

        _timeProvider.Elapse(new TimeSpan(0, 1, 0));
        _rejectedPunchItem.Reject(_currentPerson);
        context.SaveChangesAsync().Wait();
    }

    [TestMethod]
    public async Task Handle_ShouldReturnNotFound_WhenPunchItemNotFound()
    {
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var query = new GetPunchItemQuery(Guid.Empty);
        var dut = new GetPunchItemQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.NotFound, result.ResultType);
        Assert.IsNull(result.Data);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnCorrectCreatedPunchItem_WhenPunchItemCreated()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var testPunchItem = _createdPunchItem;
        var query = new GetPunchItemQuery(testPunchItem.Guid);
        var dut = new GetPunchItemQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        var punchItemDetailsDto = result.Data;
        AssertPunchItem(testPunchItem, punchItemDetailsDto);

        Assert.IsTrue(punchItemDetailsDto.IsReadyToBeCleared);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeUncleared);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeRejected);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeVerified);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeUnverified);

        AssertNotModified(punchItemDetailsDto);
        AssertNotCleared(punchItemDetailsDto);
        AssertNotVerified(punchItemDetailsDto);
        AssertNotRejected(punchItemDetailsDto);

        AssertRaisedByOrg(punchItemDetailsDto);
        AssertClearingByOrg(punchItemDetailsDto);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnCorrectModifiedPunchItem_WhenPunchItemModified()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var testPunchItem = _modifiedPunchItem;
        var query = new GetPunchItemQuery(testPunchItem.Guid);
        var dut = new GetPunchItemQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        var punchItemDetailsDto = result.Data;
        AssertPunchItem(testPunchItem, punchItemDetailsDto);

        Assert.IsTrue(punchItemDetailsDto.IsReadyToBeCleared);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeUncleared);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeRejected);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeVerified);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeUnverified);

        AssertModified(testPunchItem, punchItemDetailsDto);
        AssertNotCleared(punchItemDetailsDto);
        AssertNotVerified(punchItemDetailsDto);
        AssertNotRejected(punchItemDetailsDto);

        Assert.AreNotEqual(punchItemDetailsDto.ModifiedAtUtc, punchItemDetailsDto.CreatedAtUtc);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnCorrectClearedPunchItem_WhenPunchItemCleared()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var testPunchItem = _clearedPunchItem;
        var query = new GetPunchItemQuery(testPunchItem.Guid);
        var dut = new GetPunchItemQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        var punchItemDetailsDto = result.Data;
        AssertPunchItem(testPunchItem, punchItemDetailsDto);

        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeCleared);
        Assert.IsTrue(punchItemDetailsDto.IsReadyToBeUncleared);
        Assert.IsTrue(punchItemDetailsDto.IsReadyToBeRejected);
        Assert.IsTrue(punchItemDetailsDto.IsReadyToBeVerified);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeUnverified);

        AssertModified(testPunchItem, punchItemDetailsDto);
        AssertCleared(testPunchItem, punchItemDetailsDto);
        AssertNotVerified(punchItemDetailsDto);
        AssertNotRejected(punchItemDetailsDto);

        Assert.AreEqual(punchItemDetailsDto.ClearedAtUtc, punchItemDetailsDto.ModifiedAtUtc);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnCorrectVerifiedPunchItem_WhenPunchItemVerified()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var testPunchItem = _verifiedPunchItem;
        var query = new GetPunchItemQuery(testPunchItem.Guid);
        var dut = new GetPunchItemQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        var punchItemDetailsDto = result.Data;
        AssertPunchItem(testPunchItem, punchItemDetailsDto);

        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeCleared);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeUncleared);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeRejected);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeVerified);
        Assert.IsTrue(punchItemDetailsDto.IsReadyToBeUnverified);

        AssertModified(testPunchItem, punchItemDetailsDto);
        AssertCleared(testPunchItem, punchItemDetailsDto);
        AssertVerified(testPunchItem, punchItemDetailsDto);
        AssertNotRejected(punchItemDetailsDto);

        Assert.AreEqual(punchItemDetailsDto.VerifiedAtUtc, punchItemDetailsDto.ModifiedAtUtc);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnCorrectRejectedPunchItem_WhenPunchItemRejected()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var testPunchItem = _rejectedPunchItem;
        var query = new GetPunchItemQuery(testPunchItem.Guid);
        var dut = new GetPunchItemQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        var punchItemDetailsDto = result.Data;
        AssertPunchItem(testPunchItem, punchItemDetailsDto);

        Assert.IsTrue(punchItemDetailsDto.IsReadyToBeCleared);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeUncleared);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeRejected);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeVerified);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeUnverified);

        AssertModified(testPunchItem, punchItemDetailsDto);
        AssertNotCleared(punchItemDetailsDto);
        AssertNotVerified(punchItemDetailsDto);
        AssertRejected(punchItemDetailsDto, testPunchItem);

        Assert.AreEqual(punchItemDetailsDto.RejectedAtUtc, punchItemDetailsDto.ModifiedAtUtc);
    }

    private void AssertRejected(PunchItemDetailsDto punchItemDetailsDto, PunchItem testPunchItem)
    {
        var rejectedBy = punchItemDetailsDto.RejectedBy;
        Assert.IsNotNull(rejectedBy);
        Assert.AreEqual(_currentPerson.Guid, rejectedBy.Guid);
        Assert.IsNotNull(punchItemDetailsDto.RejectedAtUtc);
        Assert.AreEqual(testPunchItem.RejectedAtUtc, punchItemDetailsDto.RejectedAtUtc);
    }

    private void AssertPunchItem(PunchItem punchItem, PunchItemDetailsDto punchItemDetailsDto)
    {
        Assert.AreEqual(punchItem.ItemNo, punchItemDetailsDto.ItemNo);
        Assert.AreEqual(punchItem.Description, punchItemDetailsDto.Description);
        Assert.AreEqual(punchItem.RowVersion.ConvertToString(), punchItemDetailsDto.RowVersion);
        var project = GetProjectById(punchItem.ProjectId);
        Assert.AreEqual(project.Name, punchItemDetailsDto.ProjectName);

        var createdBy = punchItemDetailsDto.CreatedBy;
        Assert.IsNotNull(createdBy);
        Assert.AreEqual(CurrentUserOid, createdBy.Guid);
        Assert.AreEqual(punchItem.CreatedAtUtc, punchItemDetailsDto.CreatedAtUtc);
    }

    private static void AssertNotModified(PunchItemDetailsDto punchItemDetailsDto)
    {
        Assert.IsNull(punchItemDetailsDto.ModifiedBy);
        Assert.IsNull(punchItemDetailsDto.ModifiedAtUtc);
    }

    private void AssertModified(PunchItem punchItem, PunchItemDetailsDto punchItemDetailsDto)
    {
        var modifiedBy = punchItemDetailsDto.ModifiedBy;
        Assert.IsNotNull(modifiedBy);
        Assert.AreEqual(CurrentUserOid, modifiedBy.Guid);
        Assert.IsNotNull(punchItemDetailsDto.ModifiedAtUtc);
        Assert.AreEqual(punchItem.ModifiedAtUtc, punchItemDetailsDto.ModifiedAtUtc);
    }

    private static void AssertNotRejected(PunchItemDetailsDto punchItemDetailsDto)
    {
        Assert.IsNull(punchItemDetailsDto.RejectedBy);
        Assert.IsNull(punchItemDetailsDto.RejectedAtUtc);
    }

    private static void AssertNotVerified(PunchItemDetailsDto punchItemDetailsDto)
    {
        Assert.IsNull(punchItemDetailsDto.VerifiedBy);
        Assert.IsNull(punchItemDetailsDto.VerifiedAtUtc);
    }

    private static void AssertNotCleared(PunchItemDetailsDto punchItemDetailsDto)
    {
        Assert.IsNull(punchItemDetailsDto.ClearedBy);
        Assert.IsNull(punchItemDetailsDto.ClearedAtUtc);
    }

    private void AssertCleared(PunchItem punchItem, PunchItemDetailsDto punchItemDetailsDto)
    {
        var clearedBy = punchItemDetailsDto.ClearedBy;
        Assert.IsNotNull(clearedBy);
        Assert.AreEqual(_currentPerson.Guid, clearedBy.Guid);
        Assert.IsNotNull(punchItemDetailsDto.ClearedAtUtc);
        Assert.AreEqual(punchItem.ClearedAtUtc, punchItemDetailsDto.ClearedAtUtc);
    }

    private void AssertVerified(PunchItem punchItem, PunchItemDetailsDto punchItemDetailsDto)
    {
        var verifiedBy = punchItemDetailsDto.VerifiedBy;
        Assert.IsNotNull(verifiedBy);
        Assert.AreEqual(_currentPerson.Guid, verifiedBy.Guid);
        Assert.IsNotNull(punchItemDetailsDto.VerifiedAtUtc);
        Assert.AreEqual(punchItem.VerifiedAtUtc, punchItemDetailsDto.VerifiedAtUtc);
        Assert.AreNotEqual(punchItemDetailsDto.VerifiedAtUtc, punchItemDetailsDto.ClearedAtUtc);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnPunchItem_WithPriority()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var testPunchItem = _punchItemWithPriority;
        var query = new GetPunchItemQuery(testPunchItem.Guid);
        var dut = new GetPunchItemQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        var libraryItemDto = result.Data.Priority;
        Assert.IsNotNull(libraryItemDto);
        AssertLibraryItem(_priority, libraryItemDto);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnPunchItem_WithSorting()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var testPunchItem = _punchItemWithSorting;
        var query = new GetPunchItemQuery(testPunchItem.Guid);
        var dut = new GetPunchItemQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        var libraryItemDto = result.Data.Sorting;
        Assert.IsNotNull(libraryItemDto);
        AssertLibraryItem(_sorting, libraryItemDto);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnPunchItem_WithType()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var testPunchItem = _punchItemWithType;
        var query = new GetPunchItemQuery(testPunchItem.Guid);
        var dut = new GetPunchItemQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        var libraryItemDto = result.Data.Type;
        Assert.IsNotNull(libraryItemDto);
        AssertLibraryItem(_type, libraryItemDto);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnPunchItem_WithoutPriority()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var testPunchItem = _punchItemWithoutPriority;
        var query = new GetPunchItemQuery(testPunchItem.Guid);
        var dut = new GetPunchItemQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        var libraryItemDto = result.Data.Priority;
        Assert.IsNull(libraryItemDto);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnPunchItem_WithoutSorting()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var testPunchItem = _punchItemWithoutSorting;
        var query = new GetPunchItemQuery(testPunchItem.Guid);
        var dut = new GetPunchItemQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        var libraryItemDto = result.Data.Sorting;
        Assert.IsNull(libraryItemDto);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnPunchItem_WithoutType()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var testPunchItem = _punchItemWithoutType;
        var query = new GetPunchItemQuery(testPunchItem.Guid);
        var dut = new GetPunchItemQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        var libraryItemDto = result.Data.Type;
        Assert.IsNull(libraryItemDto);
    }

    private void AssertRaisedByOrg(PunchItemDetailsDto punchItemDetailsDto)
    {
        var libraryItemDto = punchItemDetailsDto.RaisedByOrg;
        Assert.IsNotNull(libraryItemDto);
        AssertLibraryItem(_raisedByOrg, libraryItemDto);
    }

    private void AssertClearingByOrg(PunchItemDetailsDto punchItemDetailsDto)
    {
        var libraryItemDto = punchItemDetailsDto.ClearingByOrg;
        Assert.IsNotNull(libraryItemDto);
        AssertLibraryItem(_clearingByOrg, libraryItemDto);
    }

    private void AssertLibraryItem(LibraryItem libraryItem, LibraryItemDto libraryItemDto)
    {
        Assert.AreEqual(libraryItem.Guid, libraryItemDto.Guid);
        Assert.AreEqual(libraryItem.Code, libraryItemDto.Code);
        Assert.AreEqual(libraryItem.Description, libraryItemDto.Description);
    }
}
