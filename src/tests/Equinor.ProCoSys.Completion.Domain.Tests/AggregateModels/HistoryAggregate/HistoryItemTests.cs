using System;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
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
    public void AddProperty_ShouldAddProperty()
    {
        // Arrange
        var dut = new HistoryItem(
            _eventForGuid,
            _eventDisplayName,
            _eventByOid,
            _eventByFullName,
            _eventAtUtc,
            _eventForParentGuid);

        var property = new Property("X", "D");
        
        // Act
        dut.AddProperty(property);
        
        // Assert
        Assert.AreEqual(1, dut.Properties.Count);
        Assert.AreEqual(property, dut.Properties.ElementAt(0));
    }

}
