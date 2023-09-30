using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
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
    private PunchItem _punchItemWithDocument;
    private PunchItem _punchItemWithWorkOrder;
    private PunchItem _punchItemWithOriginalWorkOrder;
    private PunchItem _punchItemWithSWCR;
    private PunchItem _punchItemWithoutPriority;
    private PunchItem _punchItemWithoutSorting;
    private PunchItem _punchItemWithoutType;
    private PunchItem _punchItemWithoutDocument;
    private PunchItem _punchItemWithoutWorkOrder;
    private PunchItem _punchItemWithoutOriginalWorkOrder;
    private PunchItem _punchItemWithoutSWCR;
    private LibraryItem _raisedByOrg;
    private LibraryItem _clearingByOrg;
    private LibraryItem _priority;
    private LibraryItem _sorting;
    private LibraryItem _type;
    private Person _currentPerson;
    private Document _document;
    private SWCR _swcr;
    private WorkOrder _workOrder;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        _currentPerson = context.Persons.Single(p => p.Guid == CurrentUserOid);
        var projectA = context.Projects.Single(p => p.Id == _projectAId);
        _raisedByOrg = context.Library.Single(l => l.Id == _raisedByOrgId);
        _clearingByOrg = context.Library.Single(l => l.Id == _clearingByOrgId);
        _priority = context.Library.Single(l => l.Id == _priorityId);
        _sorting = context.Library.Single(l => l.Id == _sortingId);
        _type = context.Library.Single(l => l.Id == _typeId);
        _document = context.Documents.Single(d => d.Id == _documentId);
        _swcr = context.SWCRs.Single(s => s.Id == _swcrId);
        _workOrder = context.WorkOrders.Single(w => w.Id == _workOrderId);

        _createdPunchItem = new PunchItem(TestPlantA, projectA, Guid.NewGuid(), Category.PB, "Desc", _raisedByOrg, _clearingByOrg);
        _createdPunchItem.SetPriority(_priority);
        _createdPunchItem.SetSorting(_sorting);
        _createdPunchItem.SetType(_type);
        _createdPunchItem.SetDocument(_document);
        _createdPunchItem.SetWorkOrder(_workOrder);
        _createdPunchItem.SetOriginalWorkOrder(_workOrder);
        _createdPunchItem.SetSWCR(_swcr);

        _modifiedPunchItem = new PunchItem(TestPlantA, projectA, Guid.NewGuid(), Category.PB, "Desc", _raisedByOrg, _clearingByOrg);
        _clearedPunchItem = new PunchItem(TestPlantA, projectA, Guid.NewGuid(), Category.PB, "Desc", _raisedByOrg, _clearingByOrg);
        _verifiedPunchItem = new PunchItem(TestPlantA, projectA, Guid.NewGuid(), Category.PB, "Desc", _raisedByOrg, _clearingByOrg);
        _rejectedPunchItem = new PunchItem(TestPlantA, projectA, Guid.NewGuid(), Category.PB, "Desc", _raisedByOrg, _clearingByOrg);

        _punchItemWithPriority = _punchItemWithSorting = _punchItemWithType =
            _punchItemWithDocument = _punchItemWithWorkOrder = _punchItemWithOriginalWorkOrder =
                _punchItemWithSWCR = _createdPunchItem;
        _punchItemWithoutPriority = _punchItemWithoutSorting = _punchItemWithoutType =
            _punchItemWithoutDocument = _punchItemWithoutWorkOrder = _punchItemWithoutOriginalWorkOrder =
                _punchItemWithoutSWCR = _modifiedPunchItem;

        context.PunchItems.Add(_createdPunchItem);
        context.PunchItems.Add(_modifiedPunchItem);
        context.PunchItems.Add(_clearedPunchItem);
        context.PunchItems.Add(_verifiedPunchItem);
        context.PunchItems.Add(_rejectedPunchItem);
        context.SaveChangesAsync().Wait();

        // Elapse some time between each update and save to be able to that 
        // timestamps of Created, Modified, Cleared and Verified differ
        _timeProvider.Elapse(new TimeSpan(0, 1, 0));
        _modifiedPunchItem.Description = "Modified";
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
    public async Task Handle_ShouldThrowException_WhenUnknownPunch()
    {
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var query = new GetPunchItemQuery(Guid.Empty);
        var dut = new GetPunchItemQueryHandler(context);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => dut.Handle(query, default));
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
    public async Task Handle_ShouldReturnPunchItem_WithDocument()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var testPunchItem = _punchItemWithDocument;
        var query = new GetPunchItemQuery(testPunchItem.Guid);
        var dut = new GetPunchItemQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        var documentDto = result.Data.Document;
        Assert.IsNotNull(documentDto);
        AssertDocument(_document, documentDto);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnPunchItem_WithWorkOrder()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var testPunchItem = _punchItemWithWorkOrder;
        var query = new GetPunchItemQuery(testPunchItem.Guid);
        var dut = new GetPunchItemQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        var workOrderDto = result.Data.WorkOrder;
        Assert.IsNotNull(workOrderDto);
        AssertWorkOrder(_workOrder, workOrderDto);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnPunchItem_WithOriginalWorkOrder()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var testPunchItem = _punchItemWithOriginalWorkOrder;
        var query = new GetPunchItemQuery(testPunchItem.Guid);
        var dut = new GetPunchItemQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        var originalWorkOrderDto = result.Data.OriginalWorkOrder;
        Assert.IsNotNull(originalWorkOrderDto);
        AssertWorkOrder(_workOrder, originalWorkOrderDto);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnPunchItem_WithSWCR()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var testPunchItem = _punchItemWithSWCR;
        var query = new GetPunchItemQuery(testPunchItem.Guid);
        var dut = new GetPunchItemQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        var swcrDto = result.Data.SWCR;
        Assert.IsNotNull(swcrDto);
        AssertSWCR(_swcr, swcrDto);
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

        Assert.IsNull(result.Data.Priority);
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

        Assert.IsNull(result.Data.Sorting);
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

        Assert.IsNull(result.Data.Type);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnPunchItem_WithoutDocument()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var testPunchItem = _punchItemWithoutDocument;
        var query = new GetPunchItemQuery(testPunchItem.Guid);
        var dut = new GetPunchItemQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        Assert.IsNull(result.Data.Document);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnPunchItem_WithoutOriginalWorkOrder()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var testPunchItem = _punchItemWithoutOriginalWorkOrder;
        var query = new GetPunchItemQuery(testPunchItem.Guid);
        var dut = new GetPunchItemQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        Assert.IsNull(result.Data.OriginalWorkOrder);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnPunchItem_WithoutSWCR()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var testPunchItem = _punchItemWithoutSWCR;
        var query = new GetPunchItemQuery(testPunchItem.Guid);
        var dut = new GetPunchItemQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        Assert.IsNull(result.Data.SWCR);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnPunchItem_WithoutWorkOrder()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var testPunchItem = _punchItemWithoutWorkOrder;
        var query = new GetPunchItemQuery(testPunchItem.Guid);
        var dut = new GetPunchItemQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        Assert.IsNull(result.Data.WorkOrder);
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
        Assert.AreEqual(punchItem.Category.ToString(), punchItemDetailsDto.Category);
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

    private void AssertDocument(Document document, DocumentDto documentDto)
    {
        Assert.AreEqual(document.Guid, documentDto.Guid);
        Assert.AreEqual(document.No, documentDto.No);
    }

    private void AssertWorkOrder(WorkOrder workOrder, WorkOrderDto workOrderDto)
    {
        Assert.AreEqual(workOrder.Guid, workOrderDto.Guid);
        Assert.AreEqual(workOrder.No, workOrderDto.No);
    }

    private void AssertSWCR(SWCR swcr, SWCRDto swcrDto)
    {
        Assert.AreEqual(swcr.Guid, swcrDto.Guid);
        Assert.AreEqual(swcr.No, swcrDto.No);
    }
}
