using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Query.Tests.PunchItemQueries.PunchItemServices;

[TestClass]
public class PunchItemServiceTests : ReadOnlyTestsBase
{
    private readonly string _testPlant = TestPlantA;

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
    private readonly Guid _checkListGuid = Guid.NewGuid();

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        _currentPerson = context.Persons.Single(p => p.Guid == CurrentUserOid);
        var projectA = context.Projects.Single(p => p.Id == _projectAId[_testPlant]);
        _raisedByOrg = context.Library.Single(l => l.Id == _raisedByOrgId[_testPlant]);
        _clearingByOrg = context.Library.Single(l => l.Id == _clearingByOrgId[_testPlant]);
        _priority = context.Library.Single(l => l.Id == _priorityId[_testPlant]);
        _sorting = context.Library.Single(l => l.Id == _sortingId[_testPlant]);
        _type = context.Library.Single(l => l.Id == _typeId[_testPlant]);
        _document = context.Documents.Single(d => d.Id == _documentId[_testPlant]);
        _swcr = context.SWCRs.Single(s => s.Id == _swcrId[_testPlant]);
        _workOrder = context.WorkOrders.Single(w => w.Id == _workOrderId[_testPlant]);

        _createdPunchItem = new PunchItem(_testPlant, projectA, _checkListGuid, Category.PB, "Desc", _raisedByOrg, _clearingByOrg);
        _createdPunchItem.SetPriority(_priority);
        _createdPunchItem.SetSorting(_sorting);
        _createdPunchItem.SetType(_type);
        _createdPunchItem.SetDocument(_document);
        _createdPunchItem.SetWorkOrder(_workOrder);
        _createdPunchItem.SetOriginalWorkOrder(_workOrder);
        _createdPunchItem.SetSWCR(_swcr);

        _modifiedPunchItem = new PunchItem(_testPlant, projectA, _checkListGuid, Category.PB, "Desc", _raisedByOrg, _clearingByOrg);
        _clearedPunchItem = new PunchItem(_testPlant, projectA, Guid.NewGuid(), Category.PB, "Desc", _raisedByOrg, _clearingByOrg);
        _verifiedPunchItem = new PunchItem(_testPlant, projectA, Guid.NewGuid(), Category.PB, "Desc", _raisedByOrg, _clearingByOrg);
        _rejectedPunchItem = new PunchItem(_testPlant, projectA, Guid.NewGuid(), Category.PB, "Desc", _raisedByOrg, _clearingByOrg);

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

        context.Attachments.Add(new Attachment(projectA.Name, nameof(PunchItem), _createdPunchItem.Guid, "file.txt",
            Guid.NewGuid()));
        context.Attachments.Add(new Attachment(projectA.Name, nameof(PunchItem), _createdPunchItem.Guid, "pic.img",
            Guid.NewGuid()));
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
    public async Task GetByCheckListGuid_ShouldReturnPunchItems_WithCorrectAttachmentCount_ForGivenCheckListGuid()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemService(context);

        // Act
        var result = await dut.GetByCheckListGuid(_checkListGuid, default);

        // Assert
        var dtos = result.ToList();
        Assert.IsTrue(2 == dtos.Count(x => x.CheckListGuid == _checkListGuid));
        Assert.IsTrue(2 == dtos.First(x => x.Guid == _createdPunchItem.Guid).AttachmentCount);
        Assert.IsTrue(0 == dtos.First(x => x.Guid == _modifiedPunchItem.Guid).AttachmentCount);
    }

    #region GetPunchItemOrNullByPunchItemGuid
    [TestMethod]
    public async Task GetPunchItemOrNullByPunchItemGuid_ShouldReturnCorrectCreatedPunchItem_WhenPunchItemCreated()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _createdPunchItem;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDto = await dut.GetPunchItemOrNullByPunchItemGuidAsync(testPunchItem.Guid, default);

        // Assert
        Assert.IsNotNull(punchItemDetailsDto);

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

        Assert.AreEqual(2, punchItemDetailsDto.AttachmentCount);
    }

    [TestMethod]
    public async Task GetPunchItemOrNullByPunchItemGuid_ShouldReturnCorrectModifiedPunchItem_WhenPunchItemModified()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _modifiedPunchItem;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDto = await dut.GetPunchItemOrNullByPunchItemGuidAsync(testPunchItem.Guid, default);

        // Assert
        Assert.IsNotNull(punchItemDetailsDto);
        
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

        Assert.AreEqual(0, punchItemDetailsDto.AttachmentCount);
    }

    [TestMethod]
    public async Task GetPunchItemOrNullByPunchItemGuid_ShouldReturnCorrectClearedPunchItem_WhenPunchItemCleared()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _clearedPunchItem;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDto = await dut.GetPunchItemOrNullByPunchItemGuidAsync(testPunchItem.Guid, default);

        // Assert
        Assert.IsNotNull(punchItemDetailsDto);

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
    public async Task GetPunchItemOrNullByPunchItemGuid_ShouldReturnCorrectVerifiedPunchItem_WhenPunchItemVerified()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _verifiedPunchItem;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDto = await dut.GetPunchItemOrNullByPunchItemGuidAsync(testPunchItem.Guid, default);

        // Assert
        Assert.IsNotNull(punchItemDetailsDto);

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
    public async Task GetPunchItemOrNullByPunchItemGuid_ShouldReturnCorrectRejectedPunchItem_WhenPunchItemRejected()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _rejectedPunchItem;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDto = await dut.GetPunchItemOrNullByPunchItemGuidAsync(testPunchItem.Guid, default);

        // Assert
        Assert.IsNotNull(punchItemDetailsDto);

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
    public async Task GetPunchItemOrNullByPunchItemGuid_ShouldReturnPunchItem_WithPriority()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithPriority;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDto = await dut.GetPunchItemOrNullByPunchItemGuidAsync(testPunchItem.Guid, default);

        // Assert
        Assert.IsNotNull(punchItemDetailsDto);

        var libraryItemDto = punchItemDetailsDto.Priority;
        Assert.IsNotNull(libraryItemDto);
        AssertLibraryItem(_priority, libraryItemDto);
    }

    [TestMethod]
    public async Task GetPunchItemOrNullByPunchItemGuid_ShouldReturnPunchItem_WithSorting()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithSorting;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDto = await dut.GetPunchItemOrNullByPunchItemGuidAsync(testPunchItem.Guid, default);

        // Assert
        Assert.IsNotNull(punchItemDetailsDto);

        var libraryItemDto = punchItemDetailsDto.Sorting;
        Assert.IsNotNull(libraryItemDto);
        AssertLibraryItem(_sorting, libraryItemDto);
    }

    [TestMethod]
    public async Task GetPunchItemOrNullByPunchItemGuid_ShouldReturnPunchItem_WithType()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithType;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDto = await dut.GetPunchItemOrNullByPunchItemGuidAsync(testPunchItem.Guid, default);

        // Assert
        Assert.IsNotNull(punchItemDetailsDto);

        var libraryItemDto = punchItemDetailsDto.Type;
        Assert.IsNotNull(libraryItemDto);
        AssertLibraryItem(_type, libraryItemDto);
    }

    [TestMethod]
    public async Task GetPunchItemOrNullByPunchItemGuid_ShouldReturnPunchItem_WithDocument()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithDocument;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDto = await dut.GetPunchItemOrNullByPunchItemGuidAsync(testPunchItem.Guid, default);

        // Assert
        Assert.IsNotNull(punchItemDetailsDto);

        var documentDto = punchItemDetailsDto.Document;
        Assert.IsNotNull(documentDto);
        AssertDocument(_document, documentDto);
    }

    [TestMethod]
    public async Task GetPunchItemOrNullByPunchItemGuid_ShouldReturnPunchItem_WithWorkOrder()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithWorkOrder;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDto = await dut.GetPunchItemOrNullByPunchItemGuidAsync(testPunchItem.Guid, default);

        // Assert
        Assert.IsNotNull(punchItemDetailsDto);

        var workOrderDto = punchItemDetailsDto.WorkOrder;
        Assert.IsNotNull(workOrderDto);
        AssertWorkOrder(_workOrder, workOrderDto);
    }

    [TestMethod]
    public async Task GetPunchItemOrNullByPunchItemGuid_ShouldReturnPunchItem_WithOriginalWorkOrder()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithOriginalWorkOrder;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDto = await dut.GetPunchItemOrNullByPunchItemGuidAsync(testPunchItem.Guid, default);

        // Assert
        Assert.IsNotNull(punchItemDetailsDto);

        var originalWorkOrderDto = punchItemDetailsDto.OriginalWorkOrder;
        Assert.IsNotNull(originalWorkOrderDto);
        AssertWorkOrder(_workOrder, originalWorkOrderDto);
    }

    [TestMethod]
    public async Task GetPunchItemOrNullByPunchItemGuid_ShouldReturnPunchItem_WithSWCR()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithSWCR;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDto = await dut.GetPunchItemOrNullByPunchItemGuidAsync(testPunchItem.Guid, default);

        // Assert
        Assert.IsNotNull(punchItemDetailsDto);

        var swcrDto = punchItemDetailsDto.SWCR;
        Assert.IsNotNull(swcrDto);
        AssertSWCR(_swcr, swcrDto);
    }

    [TestMethod]
    public async Task GetPunchItemOrNullByPunchItemGuid_ShouldReturnPunchItem_WithoutPriority()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithoutPriority;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDto = await dut.GetPunchItemOrNullByPunchItemGuidAsync(testPunchItem.Guid, default);

        // Assert
        Assert.IsNotNull(punchItemDetailsDto);

        Assert.IsNull(punchItemDetailsDto.Priority);
    }

    [TestMethod]
    public async Task GetPunchItemOrNullByPunchItemGuid_ShouldReturnPunchItem_WithoutSorting()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithoutSorting;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDto = await dut.GetPunchItemOrNullByPunchItemGuidAsync(testPunchItem.Guid, default);

        // Assert
        Assert.IsNotNull(punchItemDetailsDto);

        Assert.IsNull(punchItemDetailsDto.Sorting);
    }

    [TestMethod]
    public async Task GetPunchItemOrNullByPunchItemGuid_ShouldReturnPunchItem_WithoutType()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithoutType;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDto = await dut.GetPunchItemOrNullByPunchItemGuidAsync(testPunchItem.Guid, default);

        // Assert
        Assert.IsNotNull(punchItemDetailsDto);

        Assert.IsNull(punchItemDetailsDto.Type);
    }

    [TestMethod]
    public async Task GetPunchItemOrNullByPunchItemGuid_ShouldReturnPunchItem_WithoutDocument()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithoutDocument;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDto = await dut.GetPunchItemOrNullByPunchItemGuidAsync(testPunchItem.Guid, default);

        // Assert
        Assert.IsNotNull(punchItemDetailsDto);

        Assert.IsNull(punchItemDetailsDto.Document);
    }

    [TestMethod]
    public async Task GetPunchItemOrNullByPunchItemGuid_ShouldReturnPunchItem_WithoutOriginalWorkOrder()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithoutOriginalWorkOrder;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDto = await dut.GetPunchItemOrNullByPunchItemGuidAsync(testPunchItem.Guid, default);

        // Assert
        Assert.IsNotNull(punchItemDetailsDto);

        Assert.IsNull(punchItemDetailsDto.OriginalWorkOrder);
    }

    [TestMethod]
    public async Task GetPunchItemOrNullByPunchItemGuid_ShouldReturnPunchItem_WithoutSWCR()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithoutSWCR;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDto = await dut.GetPunchItemOrNullByPunchItemGuidAsync(testPunchItem.Guid, default);

        // Assert
        Assert.IsNotNull(punchItemDetailsDto);

        Assert.IsNull(punchItemDetailsDto.SWCR);
    }

    [TestMethod]
    public async Task GetPunchItemOrNullByPunchItemGuid_ShouldReturnPunchItem_WithoutWorkOrder()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithoutWorkOrder;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDto = await dut.GetPunchItemOrNullByPunchItemGuidAsync(testPunchItem.Guid, default);

        // Assert
        Assert.IsNotNull(punchItemDetailsDto);

        Assert.IsNull(punchItemDetailsDto.WorkOrder);
    }

    [TestMethod]
    public async Task GetPunchItemOrNullByPunchItemGuid_ShouldReturnNull_WhenUnknownPunch()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDto = await dut.GetPunchItemOrNullByPunchItemGuidAsync(Guid.NewGuid(), default);

        // Assert
        Assert.IsNull(punchItemDetailsDto);
    }
    #endregion

    #region GetPunchItemsByPunchItemGuids
    [TestMethod]
    public async Task GetPunchItemsByPunchItemGuids_ShouldReturnManyPunchItems()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem1 = _createdPunchItem;
        var testPunchItem2 = _modifiedPunchItem;
        var testPunchItem3 = _clearedPunchItem;
        var testPunchItem4 = _verifiedPunchItem;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDtos = await dut.GetPunchItemsByPunchItemGuidsAsync(
            [testPunchItem1.Guid, testPunchItem2.Guid, testPunchItem3.Guid, testPunchItem4.Guid], default);

        // Assert
        var dtoList = punchItemDetailsDtos.ToList();
        Assert.AreEqual(4, dtoList.Count);
        Assert.IsNotNull(dtoList.SingleOrDefault(p => p.Guid == testPunchItem1.Guid));
        Assert.IsNotNull(dtoList.SingleOrDefault(p => p.Guid == testPunchItem2.Guid));
        Assert.IsNotNull(dtoList.SingleOrDefault(p => p.Guid == testPunchItem3.Guid));
        Assert.IsNotNull(dtoList.SingleOrDefault(p => p.Guid == testPunchItem4.Guid));
    }

    [TestMethod]
    public async Task GetPunchItemsByPunchItemGuids_ShouldReturnCorrectCreatedPunchItem_WhenPunchItemCreated()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _createdPunchItem;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDtos = await dut.GetPunchItemsByPunchItemGuidsAsync([testPunchItem.Guid], default);

        // Assert
        var punchItemDetailsDto = punchItemDetailsDtos.SingleOrDefault();
        Assert.IsNotNull(punchItemDetailsDto);
        AssertPunchItem(testPunchItem, punchItemDetailsDto);

        Assert.IsTrue(punchItemDetailsDto.IsReadyToBeCleared);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeUncleared);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeRejected);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeVerified);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeUnverified);

        Assert.IsNull(punchItemDetailsDto.ClearedAtUtc);
        Assert.IsNull(punchItemDetailsDto.VerifiedAtUtc);
        Assert.IsNull(punchItemDetailsDto.RejectedAtUtc);

        AssertRaisedByOrg(punchItemDetailsDto);
        AssertClearingByOrg(punchItemDetailsDto);
    }

    [TestMethod]
    public async Task GetPunchItemsByPunchItemGuids_ShouldReturnCorrectModifiedPunchItem_WhenPunchItemModified()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _modifiedPunchItem;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDtos = await dut.GetPunchItemsByPunchItemGuidsAsync([testPunchItem.Guid], default);

        // Assert
        var punchItemDetailsDto = punchItemDetailsDtos.SingleOrDefault();
        Assert.IsNotNull(punchItemDetailsDto);

        AssertPunchItem(testPunchItem, punchItemDetailsDto);

        Assert.IsTrue(punchItemDetailsDto.IsReadyToBeCleared);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeUncleared);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeRejected);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeVerified);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeUnverified);

        Assert.IsNull(punchItemDetailsDto.ClearedAtUtc);
        Assert.IsNull(punchItemDetailsDto.VerifiedAtUtc);
        Assert.IsNull(punchItemDetailsDto.RejectedAtUtc);
    }

    [TestMethod]
    public async Task GetPunchItemsByPunchItemGuids_ShouldReturnCorrectClearedPunchItem_WhenPunchItemCleared()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _clearedPunchItem;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDtos = await dut.GetPunchItemsByPunchItemGuidsAsync([testPunchItem.Guid], default);

        // Assert
        var punchItemDetailsDto = punchItemDetailsDtos.SingleOrDefault();
        Assert.IsNotNull(punchItemDetailsDto);

        AssertPunchItem(testPunchItem, punchItemDetailsDto);

        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeCleared);
        Assert.IsTrue(punchItemDetailsDto.IsReadyToBeUncleared);
        Assert.IsTrue(punchItemDetailsDto.IsReadyToBeRejected);
        Assert.IsTrue(punchItemDetailsDto.IsReadyToBeVerified);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeUnverified);

        Assert.IsNotNull(punchItemDetailsDto.ClearedAtUtc);
        Assert.IsNull(punchItemDetailsDto.VerifiedAtUtc);
        Assert.IsNull(punchItemDetailsDto.RejectedAtUtc);
    }

    [TestMethod]
    public async Task GetPunchItemsByPunchItemGuids_ShouldReturnCorrectVerifiedPunchItem_WhenPunchItemVerified()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _verifiedPunchItem;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDtos = await dut.GetPunchItemsByPunchItemGuidsAsync([testPunchItem.Guid], default);

        // Assert
        var punchItemDetailsDto = punchItemDetailsDtos.SingleOrDefault();
        Assert.IsNotNull(punchItemDetailsDto);

        AssertPunchItem(testPunchItem, punchItemDetailsDto);

        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeCleared);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeUncleared);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeRejected);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeVerified);
        Assert.IsTrue(punchItemDetailsDto.IsReadyToBeUnverified);

        Assert.IsNotNull(punchItemDetailsDto.ClearedAtUtc);
        Assert.IsNotNull(punchItemDetailsDto.VerifiedAtUtc);
        Assert.IsNull(punchItemDetailsDto.RejectedAtUtc);
    }

    [TestMethod]
    public async Task GetPunchItemsByPunchItemGuids_ShouldReturnCorrectRejectedPunchItem_WhenPunchItemRejected()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _rejectedPunchItem;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDtos = await dut.GetPunchItemsByPunchItemGuidsAsync([testPunchItem.Guid], default);

        // Assert
        var punchItemDetailsDto = punchItemDetailsDtos.SingleOrDefault();
        Assert.IsNotNull(punchItemDetailsDto);

        AssertPunchItem(testPunchItem, punchItemDetailsDto);

        Assert.IsTrue(punchItemDetailsDto.IsReadyToBeCleared);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeUncleared);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeRejected);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeVerified);
        Assert.IsFalse(punchItemDetailsDto.IsReadyToBeUnverified);
    }

    [TestMethod]
    public async Task GetPunchItemsByPunchItemGuids_ShouldReturnPunchItem_WithPriority()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithPriority;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDtos = await dut.GetPunchItemsByPunchItemGuidsAsync([testPunchItem.Guid], default);

        // Assert
        var punchItemDetailsDto = punchItemDetailsDtos.SingleOrDefault();
        Assert.IsNotNull(punchItemDetailsDto);

        var libraryItemDto = punchItemDetailsDto.Priority;
        Assert.IsNotNull(libraryItemDto);
        AssertLibraryItem(_priority, libraryItemDto);
    }

    [TestMethod]
    public async Task GetPunchItemsByPunchItemGuids_ShouldReturnPunchItem_WithSorting()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithSorting;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDtos = await dut.GetPunchItemsByPunchItemGuidsAsync([testPunchItem.Guid], default);

        // Assert
        var punchItemDetailsDto = punchItemDetailsDtos.SingleOrDefault();
        Assert.IsNotNull(punchItemDetailsDto);

        var libraryItemDto = punchItemDetailsDto.Sorting;
        Assert.IsNotNull(libraryItemDto);
        AssertLibraryItem(_sorting, libraryItemDto);
    }

    [TestMethod]
    public async Task GetPunchItemsByPunchItemGuids_ShouldReturnPunchItem_WithType()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithType;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDtos = await dut.GetPunchItemsByPunchItemGuidsAsync([testPunchItem.Guid], default);

        // Assert
        var punchItemDetailsDto = punchItemDetailsDtos.SingleOrDefault();
        Assert.IsNotNull(punchItemDetailsDto);

        var libraryItemDto = punchItemDetailsDto.Type;
        Assert.IsNotNull(libraryItemDto);
        AssertLibraryItem(_type, libraryItemDto);
    }

    [TestMethod]
    public async Task GetPunchItemsByPunchItemGuids_ShouldReturnPunchItem_WithDocument()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithDocument;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDtos = await dut.GetPunchItemsByPunchItemGuidsAsync([testPunchItem.Guid], default);

        // Assert
        var punchItemDetailsDto = punchItemDetailsDtos.SingleOrDefault();
        Assert.IsNotNull(punchItemDetailsDto);

        var documentDto = punchItemDetailsDto.Document;
        Assert.IsNotNull(documentDto);
        AssertDocument(_document, documentDto);
    }

    [TestMethod]
    public async Task GetPunchItemsByPunchItemGuids_ShouldReturnPunchItem_WithWorkOrder()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithWorkOrder;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDtos = await dut.GetPunchItemsByPunchItemGuidsAsync([testPunchItem.Guid], default);

        // Assert
        var punchItemDetailsDto = punchItemDetailsDtos.SingleOrDefault();
        Assert.IsNotNull(punchItemDetailsDto);

        var workOrderDto = punchItemDetailsDto.WorkOrder;
        Assert.IsNotNull(workOrderDto);
        AssertWorkOrder(_workOrder, workOrderDto);
    }

    [TestMethod]
    public async Task GetPunchItemsByPunchItemGuids_ShouldReturnPunchItem_WithOriginalWorkOrder()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithOriginalWorkOrder;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDtos = await dut.GetPunchItemsByPunchItemGuidsAsync([testPunchItem.Guid], default);

        // Assert
        var punchItemDetailsDto = punchItemDetailsDtos.SingleOrDefault();
        Assert.IsNotNull(punchItemDetailsDto);

        var originalWorkOrderDto = punchItemDetailsDto.OriginalWorkOrder;
        Assert.IsNotNull(originalWorkOrderDto);
        AssertWorkOrder(_workOrder, originalWorkOrderDto);
    }

    [TestMethod]
    public async Task GetPunchItemsByPunchItemGuids_ShouldReturnPunchItem_WithSWCR()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithSWCR;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDtos = await dut.GetPunchItemsByPunchItemGuidsAsync([testPunchItem.Guid], default);

        // Assert
        var punchItemDetailsDto = punchItemDetailsDtos.SingleOrDefault();
        Assert.IsNotNull(punchItemDetailsDto);

        var swcrDto = punchItemDetailsDto.SWCR;
        Assert.IsNotNull(swcrDto);
        AssertSWCR(_swcr, swcrDto);
    }

    [TestMethod]
    public async Task GetPunchItemsByPunchItemGuids_ShouldReturnPunchItem_WithoutPriority()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithoutPriority;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDtos = await dut.GetPunchItemsByPunchItemGuidsAsync([testPunchItem.Guid], default);

        // Assert
        var punchItemDetailsDto = punchItemDetailsDtos.SingleOrDefault();
        Assert.IsNotNull(punchItemDetailsDto);

        Assert.IsNull(punchItemDetailsDto.Priority);
    }

    [TestMethod]
    public async Task GetPunchItemsByPunchItemGuids_ShouldReturnPunchItem_WithoutSorting()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithoutSorting;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDtos = await dut.GetPunchItemsByPunchItemGuidsAsync([testPunchItem.Guid], default);

        // Assert
        var punchItemDetailsDto = punchItemDetailsDtos.SingleOrDefault();
        Assert.IsNotNull(punchItemDetailsDto);

        Assert.IsNull(punchItemDetailsDto.Sorting);
    }

    [TestMethod]
    public async Task GetPunchItemsByPunchItemGuids_ShouldReturnPunchItem_WithoutType()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithoutType;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDtos = await dut.GetPunchItemsByPunchItemGuidsAsync([testPunchItem.Guid], default);

        // Assert
        var punchItemDetailsDto = punchItemDetailsDtos.SingleOrDefault();
        Assert.IsNotNull(punchItemDetailsDto);

        Assert.IsNull(punchItemDetailsDto.Type);
    }

    [TestMethod]
    public async Task GetPunchItemsByPunchItemGuids_ShouldReturnPunchItem_WithoutDocument()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithoutDocument;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDtos = await dut.GetPunchItemsByPunchItemGuidsAsync([testPunchItem.Guid], default);

        // Assert
        var punchItemDetailsDto = punchItemDetailsDtos.SingleOrDefault();
        Assert.IsNotNull(punchItemDetailsDto);

        Assert.IsNull(punchItemDetailsDto.Document);
    }

    [TestMethod]
    public async Task GetPunchItemsByPunchItemGuids_ShouldReturnPunchItem_WithoutOriginalWorkOrder()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithoutOriginalWorkOrder;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDtos = await dut.GetPunchItemsByPunchItemGuidsAsync([testPunchItem.Guid], default);

        // Assert
        var punchItemDetailsDto = punchItemDetailsDtos.SingleOrDefault();
        Assert.IsNotNull(punchItemDetailsDto);

        Assert.IsNull(punchItemDetailsDto.OriginalWorkOrder);
    }

    [TestMethod]
    public async Task GetPunchItemsByPunchItemGuids_ShouldReturnPunchItem_WithoutSWCR()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithoutSWCR;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDtos = await dut.GetPunchItemsByPunchItemGuidsAsync([testPunchItem.Guid], default);

        // Assert
        var punchItemDetailsDto = punchItemDetailsDtos.SingleOrDefault();
        Assert.IsNotNull(punchItemDetailsDto);

        Assert.IsNull(punchItemDetailsDto.SWCR);
    }

    [TestMethod]
    public async Task GetPunchItemsByPunchItemGuids_ShouldReturnPunchItem_WithoutWorkOrder()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var testPunchItem = _punchItemWithoutWorkOrder;
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDtos = await dut.GetPunchItemsByPunchItemGuidsAsync([testPunchItem.Guid], default);

        // Assert
        var punchItemDetailsDto = punchItemDetailsDtos.SingleOrDefault();
        Assert.IsNotNull(punchItemDetailsDto);

        Assert.IsNull(punchItemDetailsDto.WorkOrder);
    }

    [TestMethod]
    public async Task GetPunchItemsByPunchItemGuids_ShouldReturnNull_WhenUnknownPunch()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemService(context);

        // Act
        var punchItemDetailsDtos = await dut.GetPunchItemsByPunchItemGuidsAsync([Guid.NewGuid()], default);

        // Assert
        Assert.IsFalse(punchItemDetailsDtos.Any());
    }
    #endregion

    #region GetProjectOrNullByPunchItemGuid
    [TestMethod]
    public async Task GetProjectOrNullByPunchItemGuid_ShouldReturnProjectDetails_WhenKnownPunch()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var testPunchItem = _createdPunchItem;
        var dut = new PunchItemService(context);

        // Act
        var projectDto = await dut.GetProjectOrNullByPunchItemGuidAsync(testPunchItem.Guid, default);

        // Assert
        Assert.IsNotNull(projectDto);
        Assert.AreEqual(testPunchItem.Project.Guid, projectDto.Guid);
        Assert.AreEqual(testPunchItem.Project.Name, projectDto.Name);
    }

    [TestMethod]
    public async Task GetProjectOrNullByPunchItemGuid_ShouldReturnProjectDetails_WhenUnknownPunch()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemService(context);

        // Act
        var projectDto = await dut.GetProjectOrNullByPunchItemGuidAsync(Guid.NewGuid(), default);

        // Assert
        Assert.IsNull(projectDto);
    }
    #endregion

    #region privats
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

    private void AssertPunchItem(PunchItem punchItem, PunchItemTinyDetailsDto punchItemDetailsDto)
    {
        Assert.AreEqual(punchItem.ItemNo, punchItemDetailsDto.ItemNo);
        Assert.AreEqual(punchItem.Category.ToString(), punchItemDetailsDto.Category);
        Assert.AreEqual(punchItem.Description, punchItemDetailsDto.Description);
        Assert.AreEqual(punchItem.RowVersion.ConvertToString(), punchItemDetailsDto.RowVersion);
        var project = GetProjectById(punchItem.ProjectId);
        Assert.AreEqual(project.Name, punchItemDetailsDto.ProjectName);
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

    private void AssertRaisedByOrg(PunchItemTinyDetailsDto punchItemDetailsDto)
    {
        var libraryItemDto = punchItemDetailsDto.RaisedByOrg;
        Assert.IsNotNull(libraryItemDto);
        AssertLibraryItem(_raisedByOrg, libraryItemDto);
    }

    private void AssertClearingByOrg(PunchItemTinyDetailsDto punchItemDetailsDto)
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
    #endregion
}
