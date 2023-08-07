using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
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
    private readonly string _itemDescription = "Item A";

    protected override ICreationAuditable GetCreationAuditable() => _dut;
    protected override IModificationAuditable GetModificationAuditable() => _dut;

    [TestInitialize]
    public void Setup()
    {
        _project = new Project(_testPlant, Guid.NewGuid(), "P", "D");
        _project.SetProtectedIdForTesting(123);

        _raisedByOrg = new LibraryItem(_testPlant, Guid.NewGuid(), null!, null!, null!);
        _raisedByOrg.SetProtectedIdForTesting(124);

        _clearingByOrg = new LibraryItem(_testPlant, Guid.NewGuid(), null!, null!, null!);
        _clearingByOrg.SetProtectedIdForTesting(125);

        _dut = new PunchItem(_testPlant, _project, _itemDescription, _raisedByOrg, _clearingByOrg); 
    }

    #region Constructor
    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        // Assert
        Assert.AreEqual(_testPlant, _dut.Plant);
        Assert.AreEqual(_project.Id, _dut.ProjectId);
        Assert.AreEqual(_itemDescription, _dut.Description);
        Assert.AreEqual(_raisedByOrg.Id, _dut.RaisedByOrgId);
        Assert.AreEqual(_clearingByOrg.Id, _dut.ClearingByOrgId);
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
            new PunchItem(_testPlant, new Project("OtherPlant", Guid.NewGuid(), "P", "D"), _itemDescription, _raisedByOrg, _clearingByOrg));

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenRaisedByOrgInOtherPlant()
        => Assert.ThrowsException<ArgumentException>(() =>
            new PunchItem(_testPlant, _project, _itemDescription, new LibraryItem("OtherPlant", Guid.NewGuid(), null!, null!, null!), _clearingByOrg));

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenClearingByOrgInOtherPlant()
        => Assert.ThrowsException<ArgumentException>(() =>
            new PunchItem(_testPlant, _project, _itemDescription, _raisedByOrg, new LibraryItem("OtherPlant", Guid.NewGuid(), null!, null!, null!)));
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

    #region Update
    [TestMethod]
    public void Update_ShouldUpdate()
    {
        // Act
        _dut.Update("ABC");

        // Assert
        Assert.AreEqual("ABC", _dut.Description);
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
}
