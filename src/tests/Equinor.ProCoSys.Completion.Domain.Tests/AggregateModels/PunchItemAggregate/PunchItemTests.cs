using System;
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
    private readonly int _projectId = 132;
    private Project _project;
    private readonly string _itemNo = "Item A";

    protected override ICreationAuditable GetCreationAuditable() => _dut;
    protected override IModificationAuditable GetModificationAuditable() => _dut;

    [TestInitialize]
    public void Setup()
    {
        _project = new Project(_testPlant, Guid.NewGuid(), "P", "D");
        _project.SetProtectedIdForTesting(_projectId);
        _dut = new PunchItem(_testPlant, _project, _itemNo); 
    }

    #region Constructor
    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        // Assert
        Assert.AreEqual(_testPlant, _dut.Plant);
        Assert.AreEqual(_projectId, _dut.ProjectId);
        Assert.AreEqual(_itemNo, _dut.ItemNo);
    }

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenProjectNotGiven() =>
        Assert.ThrowsException<ArgumentNullException>(() =>
            new PunchItem(_testPlant, null!, _itemNo));

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenProjectInOtherPlant()
        => Assert.ThrowsException<ArgumentException>(() =>
            new PunchItem(_testPlant, new Project("OtherPlant", Guid.NewGuid(), "P", "D"), _itemNo));
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
        _dut.Reject(_person);
        Assert.IsTrue(_dut.RejectedAtUtc.HasValue);
        Assert.IsTrue(_dut.RejectedById.HasValue);

        // Act
        _dut.Clear(_person);

        // Assert
        Assert.IsFalse(_dut.RejectedAtUtc.HasValue);
        Assert.IsFalse(_dut.RejectedById.HasValue);
    }
    #endregion

    #region Reject
    [TestMethod]
    public void Reject_ShouldSetRejectedFields()
    {
        // Arrange
        Assert.IsFalse(_dut.RejectedAtUtc.HasValue);
        Assert.IsFalse(_dut.RejectedById.HasValue);

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

        // Act
        _dut.Reject(_person);

        // Assert
        Assert.IsFalse(_dut.ClearedAtUtc.HasValue);
        Assert.IsFalse(_dut.ClearedById.HasValue);
    }
    #endregion

    #region Verify
    [TestMethod]
    public void Verify_ShouldSetVerifiedFields()
    {
        // Arrange
        Assert.IsFalse(_dut.VerifiedAtUtc.HasValue);
        Assert.IsFalse(_dut.VerifiedById.HasValue);

        // Act
        _dut.Verify(_person);

        // Assert
        Assert.AreEqual(_now, _dut.VerifiedAtUtc);
        Assert.AreEqual(_person.Id, _dut.VerifiedById);
    }
    #endregion
}
