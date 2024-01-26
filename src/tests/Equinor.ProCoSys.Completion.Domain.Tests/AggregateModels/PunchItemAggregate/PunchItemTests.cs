using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Equinor.ProCoSys.Completion.Domain.Audit;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.PunchItemAggregate;

[TestClass]
public class PunchItemTests : IModificationAuditableTests
{
    private PunchItem _dut;
    private readonly string _testPlant = "PlantA";
    private Project _project;
    private LibraryItem _raisedByOrg;
    private LibraryItem _clearingByOrg;
    private LibraryItem _priority;
    private LibraryItem _type;
    private LibraryItem _sorting;
    private WorkOrder _workOrder;
    private Document _document;
    private SWCR _swcr;
    private Person _actionBy;
    private readonly Category _itemCategory = Category.PA;
    private readonly string _itemDescription = "Item A";
    private readonly Guid _checkListGuid = Guid.NewGuid();

    protected override ICreationAuditable GetCreationAuditable() => _dut;
    protected override IModificationAuditable GetModificationAuditable() => _dut;

    [TestInitialize]
    public void Setup()
    {
        _project = new Project(_testPlant, Guid.NewGuid(), "P", "D");
        _project.SetProtectedIdForTesting(123);

        _raisedByOrg = new LibraryItem(_testPlant, Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION);
        _raisedByOrg.SetProtectedIdForTesting(124);

        _clearingByOrg = new LibraryItem(_testPlant, Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION);
        _clearingByOrg.SetProtectedIdForTesting(125);

        _priority = new LibraryItem(_testPlant, Guid.NewGuid(), null!, null!, LibraryType.PUNCHLIST_PRIORITY);
        _priority.SetProtectedIdForTesting(126);

        _type = new LibraryItem(_testPlant, Guid.NewGuid(), null!, null!, LibraryType.PUNCHLIST_TYPE);
        _type.SetProtectedIdForTesting(127);

        _sorting = new LibraryItem(_testPlant, Guid.NewGuid(), null!, null!, LibraryType.PUNCHLIST_SORTING);
        _sorting.SetProtectedIdForTesting(128);

        _workOrder = new WorkOrder(_testPlant, Guid.NewGuid(), null!);
        _workOrder.SetProtectedIdForTesting(129);

        _document = new Document(_testPlant, Guid.NewGuid(), null!);
        _document.SetProtectedIdForTesting(130);

        _swcr = new SWCR(_testPlant, Guid.NewGuid(), 1);
        _swcr.SetProtectedIdForTesting(131);

        _actionBy = new Person(Guid.NewGuid(), null!, null!, null!, null!, false);
        _actionBy.SetProtectedIdForTesting(132);

        _dut = new PunchItem(_testPlant, _project, _checkListGuid, _itemCategory, _itemDescription, _raisedByOrg,
            _clearingByOrg);
    }

    #region Constructor

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        // Assert
        Assert.AreEqual(_testPlant, _dut.Plant);
        Assert.AreEqual(_project.Id, _dut.ProjectId);
        Assert.AreEqual(_project, _dut.Project);
        Assert.AreEqual(_checkListGuid, _dut.CheckListGuid);
        Assert.AreEqual(_itemCategory, _dut.Category);
        Assert.AreEqual(_itemDescription, _dut.Description);
        Assert.AreEqual(_raisedByOrg, _dut.RaisedByOrg);
        Assert.AreEqual(_raisedByOrg.Id, _dut.RaisedByOrgId);
        Assert.AreEqual(_clearingByOrg.Id, _dut.ClearingByOrgId);
        Assert.AreEqual(_clearingByOrg, _dut.ClearingByOrg);
    }

    [TestMethod]
    public void Constructor_ShouldNotSetIdOrItemNo()
    {
        // Assert
        Assert.AreEqual(0, _dut.Id);
        Assert.AreEqual(0, _dut.ItemNo);
    }

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenProjectInOtherPlant()
        => Assert.ThrowsException<ArgumentException>(() =>
            new PunchItem(
                _testPlant,
                new Project("OtherPlant", Guid.NewGuid(), "P", "D"),
                Guid.Empty,
                Category.PA,
                _itemDescription,
                _raisedByOrg,
                _clearingByOrg));

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenRaisedByOrgInOtherPlant()
        => Assert.ThrowsException<ArgumentException>(() =>
            new PunchItem(
                _testPlant,
                _project,
                Guid.Empty,
                Category.PA,
                _itemDescription,
                new LibraryItem("OtherPlant", Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION),
                _clearingByOrg));

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenRaisedByOrgIsIncorrectType()
        => Assert.ThrowsException<ArgumentException>(() =>
            new PunchItem(
                _testPlant,
                _project,
                Guid.Empty,
                Category.PA,
                _itemDescription,
                new LibraryItem(_testPlant, Guid.NewGuid(), null!, null!, LibraryType.PUNCHLIST_TYPE),
                _clearingByOrg));

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenClearingByOrgInOtherPlant()
        => Assert.ThrowsException<ArgumentException>(() =>
            new PunchItem(
                _testPlant,
                _project,
                Guid.Empty,
                Category.PA,
                _itemDescription,
                _raisedByOrg,
                new LibraryItem("OtherPlant", Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION)));

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenClearingByOrgIsIncorrectType()
        => Assert.ThrowsException<ArgumentException>(() =>
            new PunchItem(
                _testPlant,
                _project,
                Guid.Empty,
                Category.PA,
                _itemDescription,
                _raisedByOrg,
                new LibraryItem(_testPlant, Guid.NewGuid(), null!, null!, LibraryType.PUNCHLIST_TYPE)));

    #endregion

    #region ItemNo

    [TestMethod]
    public void ItemNo_ShouldReturnId()
    {
        // Arrange
        var id = 5;
        _dut.SetProtectedIdForTesting(id);

        // Act
        var itemNo = _dut.ItemNo;

        // Assert
        Assert.AreEqual(id, itemNo);
    }

    #endregion

    #region Clear

    [TestMethod]
    public void Clear_ShouldSetClearedFields()
    {
        // Arrange
        Assert.IsFalse(_dut.ClearedAtUtc.HasValue);
        Assert.IsFalse(_dut.ClearedById.HasValue);
        Assert.IsTrue(_dut.IsReadyToBeCleared);

        // Act
        _dut.Clear(_person);

        // Assert
        Assert.AreEqual(_now, _dut.ClearedAtUtc);
        Assert.AreEqual(_person.Id, _dut.ClearedById);
    }

    [TestMethod]
    public void Clear_ShouldSetRejectedFieldsToNull()
    {
        // Arrange
        _dut.Clear(_person);
        _dut.Reject(_person);
        Assert.IsTrue(_dut.RejectedAtUtc.HasValue);
        Assert.IsTrue(_dut.RejectedById.HasValue);
        Assert.IsTrue(_dut.IsReadyToBeCleared);

        // Act
        _dut.Clear(_person);

        // Assert
        Assert.IsFalse(_dut.RejectedAtUtc.HasValue);
        Assert.IsFalse(_dut.RejectedById.HasValue);
    }

    [TestMethod]
    public void Clear_ShouldThrowException_WhenNotReadyToBeCleared()
    {
        // Arrange
        _dut.Clear(_person);
        Assert.IsFalse(_dut.IsReadyToBeCleared);

        // Act and Assert
        Assert.ThrowsException<Exception>(() => _dut.Clear(_person));
    }

    #endregion

    #region Reject

    [TestMethod]
    public void Reject_ShouldSetRejectedFields()
    {
        // Arrange
        _dut.Clear(_person);
        Assert.IsFalse(_dut.RejectedAtUtc.HasValue);
        Assert.IsFalse(_dut.RejectedById.HasValue);
        Assert.IsTrue(_dut.IsReadyToBeRejected);

        // Act
        _dut.Reject(_person);

        // Assert
        Assert.AreEqual(_now, _dut.RejectedAtUtc);
        Assert.AreEqual(_person.Id, _dut.RejectedById);
    }

    [TestMethod]
    public void Reject_ShouldSetClearedFieldsToNull()
    {
        // Arrange
        _dut.Clear(_person);
        Assert.IsTrue(_dut.ClearedAtUtc.HasValue);
        Assert.IsTrue(_dut.ClearedById.HasValue);
        Assert.IsTrue(_dut.IsReadyToBeRejected);

        // Act
        _dut.Reject(_person);

        // Assert
        Assert.IsFalse(_dut.ClearedAtUtc.HasValue);
        Assert.IsFalse(_dut.ClearedById.HasValue);
    }

    [TestMethod]
    public void Reject_ShouldThrowException_WhenNotReadyToBeRejected()
    {
        // Arrange
        Assert.IsFalse(_dut.IsReadyToBeRejected);

        // Act and Assert
        Assert.ThrowsException<Exception>(() => _dut.Reject(_person));
    }

    #endregion

    #region Verify

    [TestMethod]
    public void Verify_ShouldSetVerifiedFields()
    {
        // Arrange
        _dut.Clear(_person);
        Assert.IsFalse(_dut.VerifiedAtUtc.HasValue);
        Assert.IsFalse(_dut.VerifiedById.HasValue);
        Assert.IsTrue(_dut.IsReadyToBeVerified);

        // Act
        _dut.Verify(_person);

        // Assert
        Assert.AreEqual(_now, _dut.VerifiedAtUtc);
        Assert.AreEqual(_person.Id, _dut.VerifiedById);
    }

    [TestMethod]
    public void Verify_ShouldThrowException_WhenNotReadyToBeVerified()
    {
        // Arrange
        Assert.IsFalse(_dut.IsReadyToBeVerified);

        // Act and Assert
        Assert.ThrowsException<Exception>(() => _dut.Verify(_person));
    }

    #endregion

    #region Unclear

    [TestMethod]
    public void Unclear_ShouldSetClearedFieldsToNull()
    {
        // Arrange
        _dut.Clear(_person);
        Assert.IsTrue(_dut.ClearedAtUtc.HasValue);
        Assert.IsTrue(_dut.ClearedById.HasValue);
        Assert.IsTrue(_dut.IsReadyToBeUncleared);

        // Act
        _dut.Unclear();

        // Assert
        Assert.IsFalse(_dut.ClearedAtUtc.HasValue);
        Assert.IsFalse(_dut.ClearedById.HasValue);
    }

    [TestMethod]
    public void Unclear_ShouldThrowException_WhenNotReadyToBeUncleared()
    {
        // Arrange
        Assert.IsFalse(_dut.IsReadyToBeUncleared);

        // Act and Assert
        Assert.ThrowsException<Exception>(() => _dut.Unclear());
    }

    #endregion

    #region Unverify

    [TestMethod]
    public void Unverify_ShouldSetVerifiedFields()
    {
        // Arrange
        _dut.Clear(_person);
        _dut.Verify(_person);
        Assert.IsTrue(_dut.VerifiedAtUtc.HasValue);
        Assert.IsTrue(_dut.VerifiedById.HasValue);
        Assert.IsTrue(_dut.IsReadyToBeUnverified);

        // Act
        _dut.Unverify();

        // Assert
        Assert.IsFalse(_dut.VerifiedAtUtc.HasValue);
        Assert.IsFalse(_dut.VerifiedById.HasValue);
    }

    [TestMethod]
    public void Unverify_ShouldThrowException_WhenNotReadyToBeUnverified()
    {
        // Arrange
        Assert.IsFalse(_dut.IsReadyToBeUnverified);

        // Act and Assert
        Assert.ThrowsException<Exception>(() => _dut.Unverify());
    }

    #endregion

    #region IsReadyToBeCleared

    [TestMethod]
    public void IsReadyToBeCleared_ShouldBeTrue_WhenNotCleared()
    {
        // Arrange
        Assert.IsFalse(_dut.ClearedAtUtc.HasValue);

        // Act
        var b = _dut.IsReadyToBeCleared;

        // Assert
        Assert.IsTrue(b);
    }

    [TestMethod]
    public void IsReadyToBeCleared_ShouldBeFalse_WhenCleared()
    {
        // Arrange
        _dut.Clear(_person);
        Assert.IsTrue(_dut.ClearedAtUtc.HasValue);

        // Act
        var b = _dut.IsReadyToBeCleared;

        // Assert
        Assert.IsFalse(b);
    }

    #endregion

    #region IsReadyToBeRejected

    [TestMethod]
    public void IsReadyToBeRejected_ShouldBeFalse_WhenNotCleared()
    {
        // Arrange
        Assert.IsFalse(_dut.ClearedAtUtc.HasValue);

        // Act
        var b = _dut.IsReadyToBeRejected;

        // Assert
        Assert.IsFalse(b);
    }

    [TestMethod]
    public void IsReadyToBeRejected_ShouldBeTrue_WhenCleared()
    {
        // Arrange
        _dut.Clear(_person);
        Assert.IsTrue(_dut.ClearedAtUtc.HasValue);

        // Act
        var b = _dut.IsReadyToBeRejected;

        // Assert
        Assert.IsTrue(b);
    }

    #endregion

    #region IsReadyToBeVerified

    [TestMethod]
    public void IsReadyToBeVerified_ShouldBeFalse_WhenNotCleared()
    {
        // Arrange
        Assert.IsFalse(_dut.ClearedAtUtc.HasValue);

        // Act
        var b = _dut.IsReadyToBeVerified;

        // Assert
        Assert.IsFalse(b);
    }

    [TestMethod]
    public void IsReadyToBeVerified_ShouldBeTrue_WhenCleared()
    {
        // Arrange
        _dut.Clear(_person);
        Assert.IsTrue(_dut.ClearedAtUtc.HasValue);

        // Act
        var b = _dut.IsReadyToBeVerified;

        // Assert
        Assert.IsTrue(b);
    }

    [TestMethod]
    public void IsReadyToBeVerified_ShouldBeFalse_WhenVerified()
    {
        // Arrange
        _dut.Clear(_person);
        _dut.Verify(_person);
        Assert.IsTrue(_dut.ClearedAtUtc.HasValue);
        Assert.IsTrue(_dut.VerifiedAtUtc.HasValue);

        // Act
        var b = _dut.IsReadyToBeVerified;

        // Assert
        Assert.IsFalse(b);
    }

    #endregion

    #region IsReadyToBeUncleared

    [TestMethod]
    public void IsReadyToBeUncleared_ShouldBeFalse_WhenNotCleared()
    {
        // Arrange
        Assert.IsFalse(_dut.ClearedAtUtc.HasValue);

        // Act
        var b = _dut.IsReadyToBeUncleared;

        // Assert
        Assert.IsFalse(b);
    }

    [TestMethod]
    public void IsReadyToBeUncleared_ShouldBeTrue_WhenCleared()
    {
        // Arrange
        _dut.Clear(_person);
        Assert.IsTrue(_dut.ClearedAtUtc.HasValue);

        // Act
        var b = _dut.IsReadyToBeUncleared;

        // Assert
        Assert.IsTrue(b);
    }

    #endregion

    #region IsReadyToBeUnverified

    [TestMethod]
    public void IsReadyToBeUnverified_ShouldBeFalse_WhenClearedButNotVerified()
    {
        // Arrange
        _dut.Clear(_person);
        Assert.IsTrue(_dut.ClearedAtUtc.HasValue);
        Assert.IsFalse(_dut.VerifiedAtUtc.HasValue);

        // Act
        var b = _dut.IsReadyToBeUnverified;

        // Assert
        Assert.IsFalse(b);
    }

    [TestMethod]
    public void IsReadyToBeUnverified_ShouldBeTrue_WhenVerified()
    {
        // Arrange
        _dut.Clear(_person);
        _dut.Verify(_person);
        Assert.IsTrue(_dut.ClearedAtUtc.HasValue);
        Assert.IsTrue(_dut.VerifiedAtUtc.HasValue);

        // Act
        var b = _dut.IsReadyToBeUnverified;

        // Assert
        Assert.IsTrue(b);
    }

    #endregion

    #region SetRaisedByOrg

    [TestMethod]
    public void SetRaisedByOrg_ShouldSetRaisedByOrg()
    {
        // Arrange 
        Assert.AreEqual(_raisedByOrg.Id, _dut.RaisedByOrgId);
        var newRaisedByOrg =
            new LibraryItem(_testPlant, Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION);
        newRaisedByOrg.SetProtectedIdForTesting(_raisedByOrg.Id + 1);

        // Act
        _dut.SetRaisedByOrg(newRaisedByOrg);

        // Assert
        Assert.AreEqual(newRaisedByOrg.Id, _dut.RaisedByOrgId);
        Assert.AreEqual(newRaisedByOrg, _dut.RaisedByOrg);
    }

    [TestMethod]
    public void SetRaisedByOrg_ShouldThrowException_WhenRaisedByOrgInOtherPlant() =>
        Assert.ThrowsException<ArgumentException>(() =>
            _dut.SetRaisedByOrg(
                new LibraryItem("OtherPlant", Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION)));

    [TestMethod]
    public void SetRaisedByOrg_ShouldThrowException_WhenRaisedByOrgIsIncorrectType() =>
        Assert.ThrowsException<ArgumentException>(() =>
            _dut.SetRaisedByOrg(
                new LibraryItem(_testPlant, Guid.NewGuid(), null!, null!, LibraryType.PUNCHLIST_TYPE)));

    #endregion

    #region SetClearingByOrg

    [TestMethod]
    public void SetClearingByOrg_ShouldSetClearingByOrg()
    {
        // Arrange 
        Assert.AreEqual(_clearingByOrg.Id, _dut.ClearingByOrgId);
        var newClearingByOrg =
            new LibraryItem(_testPlant, Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION);
        newClearingByOrg.SetProtectedIdForTesting(newClearingByOrg.Id + 1);
        // Act
        _dut.SetClearingByOrg(newClearingByOrg);

        // Assert
        Assert.AreEqual(newClearingByOrg.Id, _dut.ClearingByOrgId);
        Assert.AreEqual(newClearingByOrg, _dut.ClearingByOrg);
    }

    [TestMethod]
    public void SetClearingByOrg_ShouldThrowException_WhenClearingByOrgInOtherPlant() =>
        Assert.ThrowsException<ArgumentException>(() =>
            _dut.SetClearingByOrg(
                new LibraryItem("OtherPlant", Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION)));

    [TestMethod]
    public void SetClearingByOrg_ShouldThrowException_WhenClearingByOrgIsIncorrectType() =>
        Assert.ThrowsException<ArgumentException>(() =>
            _dut.SetClearingByOrg(
                new LibraryItem(_testPlant, Guid.NewGuid(), null!, null!, LibraryType.PUNCHLIST_TYPE)));

    #endregion

    #region SetPriority

    [TestMethod]
    public void SetPriority_ShouldSetPriority()
    {
        // Arrange
        Assert.IsNull(_dut.PriorityId);

        // Act
        _dut.SetPriority(_priority);

        // Assert
        Assert.AreEqual(_priority.Id, _dut.PriorityId);
        Assert.AreEqual(_priority, _dut.Priority);
    }

    [TestMethod]
    public void SetPriority_ShouldThrowException_WhenPriorityInOtherPlant() =>
        Assert.ThrowsException<ArgumentException>(() =>
            _dut.SetPriority(
                new LibraryItem("OtherPlant", Guid.NewGuid(), null!, null!, LibraryType.PUNCHLIST_PRIORITY)));

    [TestMethod]
    public void SetPriority_ShouldThrowException_WhenPriorityIsIncorrectType() =>
        Assert.ThrowsException<ArgumentException>(() =>
            _dut.SetPriority(
                new LibraryItem(_testPlant, Guid.NewGuid(), null!, null!, LibraryType.PUNCHLIST_TYPE)));

    #endregion

    #region SetSorting

    [TestMethod]
    public void SetSorting_ShouldSetSorting()
    {
        // Arrange
        Assert.IsNull(_dut.SortingId);

        // Act
        _dut.SetSorting(_sorting);

        // Assert
        Assert.AreEqual(_sorting.Id, _dut.SortingId);
        Assert.AreEqual(_sorting, _dut.Sorting);
    }

    [TestMethod]
    public void SetSorting_ShouldThrowException_WhenSortingInOtherPlant() =>
        Assert.ThrowsException<ArgumentException>(() =>
            _dut.SetSorting(
                new LibraryItem("OtherPlant", Guid.NewGuid(), null!, null!, LibraryType.PUNCHLIST_SORTING)));

    [TestMethod]
    public void SetSorting_ShouldThrowException_WhenSortingIsIncorrectType() =>
        Assert.ThrowsException<ArgumentException>(() =>
            _dut.SetSorting(
                new LibraryItem(_testPlant, Guid.NewGuid(), null!, null!, LibraryType.PUNCHLIST_TYPE)));

    #endregion

    #region SetType

    [TestMethod]
    public void SetType_ShouldSetTypeId()
    {
        // Arrange
        Assert.IsNull(_dut.TypeId);

        // Act
        _dut.SetType(_type);

        // Assert
        Assert.AreEqual(_type.Id, _dut.TypeId);
        Assert.AreEqual(_type, _dut.Type);
    }

    [TestMethod]
    public void SetType_ShouldThrowException_WhenTypeInOtherPlant() =>
        Assert.ThrowsException<ArgumentException>(() =>
            _dut.SetType(
                new LibraryItem("OtherPlant", Guid.NewGuid(), null!, null!, LibraryType.PUNCHLIST_TYPE)));

    [TestMethod]
    public void SetType_ShouldThrowException_WhenTypeIsIncorrectType() =>
        Assert.ThrowsException<ArgumentException>(() =>
            _dut.SetType(
                new LibraryItem(_testPlant, Guid.NewGuid(), null!, null!, LibraryType.PUNCHLIST_SORTING)));

    #endregion

    #region ClearPriority

    [TestMethod]
    public void ClearPriority_ShouldClearPriority()
    {
        // Arrange
        _dut.SetPriority(_priority);
        Assert.AreEqual(_priority.Id, _dut.PriorityId);
        Assert.IsNotNull(_dut.Priority);

        // Act
        _dut.ClearPriority();

        // Assert
        Assert.IsNull(_dut.PriorityId);
        Assert.IsNull(_dut.Priority);
    }

    #endregion

    #region ClearSorting

    [TestMethod]
    public void ClearSorting_ShouldClearSorting()
    {
        // Arrange
        _dut.SetSorting(_sorting);
        Assert.AreEqual(_sorting.Id, _dut.SortingId);
        Assert.IsNotNull(_dut.Sorting);

        // Act
        _dut.ClearSorting();

        // Assert
        Assert.IsNull(_dut.SortingId);
        Assert.IsNull(_dut.Sorting);
    }

    #endregion

    #region ClearType

    [TestMethod]
    public void ClearType_ShouldClearType()
    {
        // Arrange
        _dut.SetType(_type);
        Assert.AreEqual(_type.Id, _dut.TypeId);
        Assert.IsNotNull(_dut.Type);

        // Act
        _dut.ClearType();

        // Assert
        Assert.IsNull(_dut.TypeId);
        Assert.IsNull(_dut.Type);
    }

    #endregion

    #region SetWorkOrder

    [TestMethod]
    public void SetWorkOrder_ShouldSetWorkOrder()
    {
        // Arrange
        Assert.IsNull(_dut.WorkOrderId);

        // Act
        _dut.SetWorkOrder(_workOrder);

        // Assert
        Assert.AreEqual(_workOrder.Id, _dut.WorkOrderId);
        Assert.AreEqual(_workOrder, _dut.WorkOrder);
    }

    [TestMethod]
    public void SetWorkOrder_ShouldThrowException_WhenWorkOrderInOtherPlant() =>
        Assert.ThrowsException<ArgumentException>(() =>
            _dut.SetWorkOrder(new WorkOrder("OtherPlant", Guid.NewGuid(), null!)));

    #endregion

    #region ClearWorkOrder

    [TestMethod]
    public void ClearWorkOrder_ShouldClearWorkOrder()
    {
        // Arrange
        _dut.SetWorkOrder(_workOrder);
        Assert.AreEqual(_workOrder.Id, _dut.WorkOrderId);
        Assert.IsNotNull(_dut.WorkOrder);

        // Act
        _dut.ClearWorkOrder();

        // Assert
        Assert.IsNull(_dut.WorkOrderId);
        Assert.IsNull(_dut.WorkOrder);
    }

    #endregion

    #region SetOriginalWorkOrder

    [TestMethod]
    public void SetOriginalWorkOrder_ShouldSetOriginalWorkOrder()
    {
        // Arrange
        Assert.IsNull(_dut.OriginalWorkOrderId);

        // Act
        _dut.SetOriginalWorkOrder(_workOrder);

        // Assert
        Assert.AreEqual(_workOrder.Id, _dut.OriginalWorkOrderId);
        Assert.AreEqual(_workOrder, _dut.OriginalWorkOrder);
    }

    [TestMethod]
    public void SetOriginalWorkOrder_ShouldThrowException_WhenOriginalWorkOrderInOtherPlant() =>
        Assert.ThrowsException<ArgumentException>(() =>
            _dut.SetOriginalWorkOrder(new WorkOrder("OtherPlant", Guid.NewGuid(), null!)));

    #endregion

    #region ClearOriginalWorkOrder

    [TestMethod]
    public void ClearOriginalWorkOrder_ShouldClearOriginalWorkOrder()
    {
        // Arrange
        _dut.SetOriginalWorkOrder(_workOrder);
        Assert.AreEqual(_workOrder.Id, _dut.OriginalWorkOrderId);
        Assert.IsNotNull(_dut.OriginalWorkOrder);

        // Act
        _dut.ClearOriginalWorkOrder();

        // Assert
        Assert.IsNull(_dut.OriginalWorkOrderId);
        Assert.IsNull(_dut.OriginalWorkOrder);
    }

    #endregion

    #region SetDocument

    [TestMethod]
    public void SetDocument_ShouldSetDocument()
    {
        // Arrange
        Assert.IsNull(_dut.DocumentId);

        // Act
        _dut.SetDocument(_document);

        // Assert
        Assert.AreEqual(_document.Id, _dut.DocumentId);
        Assert.AreEqual(_document, _dut.Document);
    }

    [TestMethod]
    public void SetDocument_ShouldThrowException_WhenDocumentInOtherPlant() =>
        Assert.ThrowsException<ArgumentException>(() =>
            _dut.SetDocument(new Document("OtherPlant", Guid.NewGuid(), null!)));

    #endregion

    #region ClearDocument

    [TestMethod]
    public void ClearDocument_ShouldClearDocument()
    {
        // Arrange
        _dut.SetDocument(_document);
        Assert.AreEqual(_document.Id, _dut.DocumentId);
        Assert.IsNotNull(_dut.Document);

        // Act
        _dut.ClearDocument();

        // Assert
        Assert.IsNull(_dut.DocumentId);
        Assert.IsNull(_dut.Document);
    }

    #endregion

    #region SetSWCR

    [TestMethod]
    public void SetSWCR_ShouldSetSWCR()
    {
        // Arrange
        Assert.IsNull(_dut.SWCRId);

        // Act
        _dut.SetSWCR(_swcr);

        // Assert
        Assert.AreEqual(_swcr.Id, _dut.SWCRId);
        Assert.AreEqual(_swcr, _dut.SWCR);
    }

    [TestMethod]
    public void SetSWCR_ShouldThrowException_WhenSWCRInOtherPlant() =>
        Assert.ThrowsException<ArgumentException>(() =>
            _dut.SetSWCR(new SWCR("OtherPlant", Guid.NewGuid(), 1)));

    #endregion

    #region ClearSWCR

    [TestMethod]
    public void ClearSWCR_ShouldClearSWCR()
    {
        // Arrange
        _dut.SetSWCR(_swcr);
        Assert.AreEqual(_swcr.Id, _dut.SWCRId);
        Assert.IsNotNull(_dut.SWCR);

        // Act
        _dut.ClearSWCR();

        // Assert
        Assert.IsNull(_dut.SWCRId);
        Assert.IsNull(_dut.SWCR);
    }

    #endregion

    #region SetActionBy

    [TestMethod]
    public void SetActionBy_ShouldSetActionBy()
    {
        // Arrange
        Assert.IsNull(_dut.ActionById);

        // Act
        _dut.SetActionBy(_actionBy);

        // Assert
        Assert.AreEqual(_actionBy.Id, _dut.ActionById);
        Assert.AreEqual(_actionBy, _dut.ActionBy);
    }

    #endregion

    #region ClearActionBy

    [TestMethod]
    public void ClearActionBy_ShouldClearActionBy()
    {
        // Arrange
        _dut.SetActionBy(_actionBy);
        Assert.AreEqual(_actionBy.Id, _dut.ActionById);
        Assert.IsNotNull(_dut.ActionBy);

        // Act
        _dut.ClearActionBy();

        // Assert
        Assert.IsNull(_dut.ActionById);
        Assert.IsNull(_dut.ActionBy);
    }

    #endregion

    #region GetContextType
    [TestMethod]
    public void GetContextType_ShouldReturnNameOfPunchItem()
    {
        // Act
        var contextType = _dut.GetContextType();

        // Assert
        Assert.AreEqual(nameof(PunchItem), contextType);
    }
    #endregion

    #region GetEmailContext
    [TestMethod]
    public void GetEmailContext_ShouldReturnDynamicContext_WithThePunchAsEntity()
    {
        // Act
        var emailContext = _dut.GetEmailContext();

        // Assert
        var entity = emailContext.Entity;
        Assert.AreEqual(_dut, entity);
    }
    #endregion
}
