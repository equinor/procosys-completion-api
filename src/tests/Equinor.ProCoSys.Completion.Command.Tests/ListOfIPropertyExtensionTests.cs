using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.Events;
using Equinor.ProCoSys.Completion.MessageContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Command.Tests;

[TestClass]
public class ListOfIPropertyExtensionTests
{
    [TestMethod]
    public void AddChangeIfNotNull_ShouldNotAddChange_WhenNull()
    {
        // Arrange
        var dut = new List<IProperty>();

        // Act
        dut.AddChangeIfNotNull(null);

        // Assert
        Assert.AreEqual(0, dut.Count);
    }

    [TestMethod]
    public void AddChangeIfNotNull_ShouldAddChange_WhenNotNull()
    {
        // Arrange
        var dut = new List<IProperty>();
        var property = new Property<string>("A", null, "blah");

        // Act
        dut.AddChangeIfNotNull(property);

        // Assert
        Assert.AreEqual(1, dut.Count);
        Assert.AreEqual(property, dut.ElementAt(0));
    }
}
