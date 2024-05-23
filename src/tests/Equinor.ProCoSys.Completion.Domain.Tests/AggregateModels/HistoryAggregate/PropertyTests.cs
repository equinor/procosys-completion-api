using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.HistoryAggregate;

[TestClass]
public class PropertyTests
{
    private readonly string _name = "X";
    private readonly string _oldValue = "old";
    private readonly string _value = "v";
    private readonly string _valueDisplayType = "T";

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        // Act
        var dut = new Property(
            _name,
            _oldValue,
            _value,
            _valueDisplayType);

        // Assert
        Assert.AreEqual(_name, dut.Name);
        Assert.AreEqual(_oldValue, dut.OldValue);
        Assert.AreEqual(_value, dut.Value);
        Assert.AreEqual(_valueDisplayType, dut.ValueDisplayType);
    }

    [TestMethod]
    public void Constructor_ShouldSetNullValues()
    {
        // Act
        var dut = new Property(
            _name,
            null,
            null,
            _valueDisplayType);

        // Assert
        Assert.IsNull(dut.OldValue);
        Assert.IsNull(dut.Value);
    }
}
