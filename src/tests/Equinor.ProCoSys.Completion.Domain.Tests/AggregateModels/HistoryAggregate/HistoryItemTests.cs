using System;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.HistoryAggregate;

[TestClass]
public class HistoryItemTests
{
    private readonly Guid _eventForGuid = Guid.NewGuid();
    private readonly string _eventDisplayName = "X";
    private readonly Guid _eventByOid = Guid.NewGuid();
    private readonly string _eventByFullName = "Peter Pan";
    private readonly DateTime _eventAtUtc = DateTime.UtcNow;
    private readonly Guid _eventForParentGuid = Guid.NewGuid();

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        // Act
        var dut = new HistoryItem(
            _eventForGuid,
            _eventDisplayName,
            _eventByOid,
            _eventByFullName,
            _eventAtUtc,
            _eventForParentGuid);

        // Assert
        Assert.AreEqual(_eventForGuid, dut.EventForGuid);
        Assert.AreEqual(_eventDisplayName, dut.EventDisplayName);
        Assert.AreEqual(_eventByOid, dut.EventByOid);
        Assert.AreEqual(_eventByFullName, dut.EventByFullName);
        Assert.AreEqual(_eventAtUtc, dut.EventAtUtc);
        Assert.AreEqual(_eventForParentGuid, dut.EventForParentGuid);
        Assert.AreEqual(0, dut.Properties.Count);
    }

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenEventAtUtcNotUtc() =>
        // Act and Assert
        Assert.ThrowsException<Exception>(() => new HistoryItem(
            _eventForGuid,
            _eventDisplayName,
            _eventByOid,
            _eventByFullName,
            DateTime.Now,
            _eventForParentGuid));

    [TestMethod]
    public void AddPropertyForCreate_ShouldAddPropertyWithValue()
    {
        // Arrange
        var dut = new HistoryItem(
            _eventForGuid,
            _eventDisplayName,
            _eventByOid,
            _eventByFullName,
            _eventAtUtc,
            _eventForParentGuid);
        var propertyName = "X";
        var propertyValue = "P";
        var valueDisplayType = ValueDisplayType.IntAsText;

        // Act
        dut.AddPropertyForCreate(propertyName, propertyValue, valueDisplayType);
        
        // Assert
        Assert.AreEqual(1, dut.Properties.Count);
        var property = dut.Properties.ElementAt(0);
        Assert.AreEqual(propertyName, property.Name);
        Assert.IsNull(property.OldValue);
        Assert.AreEqual(propertyValue, property.Value);
        Assert.AreEqual(valueDisplayType.ToString(), property.ValueDisplayType);
    }

    [TestMethod]
    public void AddPropertyForCreate_ShouldAddPropertyWithNullValue()
    {
        // Arrange
        var dut = new HistoryItem(
            _eventForGuid,
            _eventDisplayName,
            _eventByOid,
            _eventByFullName,
            _eventAtUtc,
            _eventForParentGuid);

        // Act
        dut.AddPropertyForCreate("X", null, ValueDisplayType.IntAsText);

        // Assert
        Assert.AreEqual(1, dut.Properties.Count);
        var property = dut.Properties.ElementAt(0);
        Assert.IsNull(property.Value);
        Assert.IsNull(property.OldValue);
    }

    [TestMethod]
    public void AddPropertyForUpdate_ShouldAddPropertyWithValue()
    {
        // Arrange
        var dut = new HistoryItem(
            _eventForGuid,
            _eventDisplayName,
            _eventByOid,
            _eventByFullName,
            _eventAtUtc,
            _eventForParentGuid);
        var propertyName = "X";
        var propertyOldValue = "P1";
        var propertyValue = "P2";
        var valueDisplayType = ValueDisplayType.IntAsText;

        // Act
        dut.AddPropertyForUpdate(propertyName, propertyOldValue, propertyValue, valueDisplayType);

        // Assert
        Assert.AreEqual(1, dut.Properties.Count);
        var property = dut.Properties.ElementAt(0);
        Assert.AreEqual(propertyName, property.Name);
        Assert.AreEqual(propertyOldValue, property.OldValue);
        Assert.AreEqual(propertyValue, property.Value);
        Assert.AreEqual(valueDisplayType.ToString(), property.ValueDisplayType);
    }

    [TestMethod]
    public void AddPropertyForUpdate_ShouldAddPropertyWithNullValue()
    {
        // Arrange
        var dut = new HistoryItem(
            _eventForGuid,
            _eventDisplayName,
            _eventByOid,
            _eventByFullName,
            _eventAtUtc,
            _eventForParentGuid);

        // Act
        dut.AddPropertyForUpdate("X", null, null, ValueDisplayType.IntAsText);

        // Assert
        Assert.AreEqual(1, dut.Properties.Count);
        var property = dut.Properties.ElementAt(0);
        Assert.IsNull(property.Value);
        Assert.IsNull(property.OldValue);
    }
}
