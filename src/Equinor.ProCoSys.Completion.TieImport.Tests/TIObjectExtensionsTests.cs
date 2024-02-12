using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Tests;

[TestClass]

public class TIObjectExtensionTests
{

    [TestMethod]
    public void GetAttributeValueAsString_ShouldReturn_WhenAnyAttributeNameCaseUsed()
    {
        // Arrange
        var tieAttribute = new TIAttribute { Name = "AttRibuTeName", Value = "   some value   " };
        var tieObject = new TIObject { Attributes = [tieAttribute] };

        // Act
        var result = tieObject.GetAttributeValueAsString("ATTRIBUTEname");

        // Assert
        Assert.AreEqual("some value", result);
    }
}
