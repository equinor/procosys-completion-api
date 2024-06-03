using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.HistoryAggregate;

[TestClass]
public class PropertyTests
{
    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        // Arrange
        var name = "N";
        var valueDisplayType = "D";

        // Act
        var dut = new Property(name, valueDisplayType);

        // Assert
        Assert.AreEqual(name, dut.Name);
        Assert.AreEqual(valueDisplayType, dut.ValueDisplayType);
        Assert.IsNull(dut.OldValue);
        Assert.IsNull(dut.Value);
        Assert.IsNull(dut.OidValue);
        Assert.IsNull(dut.OldOidValue);
    }
}
