using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Tests;

[TestClass]

public class TIAttributeExtensionTests
{

    [TestMethod]
    public void GetValueAsString_ShouldReturnNull_WhenValueIsNull()
    {
        // Arrange
        var tieAttribute = new TIAttribute {Value = null};

        // Act
        var result = tieAttribute.GetValueAsString();

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetValueAsString_ShouldReturnNull_WhenValueIsEmptyString()
    {
        // Arrange
        var tieAttribute = new TIAttribute { Value = string.Empty };

        // Act
        var result = tieAttribute.GetValueAsString();

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetValueAsString_ShouldReturnTrimmedValue()
    {
        // Arrange
        var tieAttribute = new TIAttribute { Value = "   some value   " };

        // Act
        var result = tieAttribute.GetValueAsString();

        // Assert
        Assert.AreEqual("some value", result);
    }
}
