using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Tests;

[TestClass]
public class TiAttributeExtensionsTests
{
    [TestMethod]
    public void GetValueAsString_ShouldReturnNull_WhenAttributeValueIsNull()
    {
        var attribute = new TIAttribute {Value = null};

        var result = attribute.GetValueAsString();
        
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetValueAsString_ShouldReturnNull_WhenAttributeValueIsWhitespace()
    {
        var attribute = new TIAttribute { Value = "   " };

        var result = attribute.GetValueAsString();

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetValueAsString_ShouldReturnTrimmedString_WhenAttributeHasValue()
    {
        var attribute = new TIAttribute { Value = "   correct value   " };

        var result = attribute.GetValueAsString();

        Assert.AreEqual("correct value", result);
    }

    [TestMethod]
    public void GetValueAsStringUpperCase_ShouldReturnNull_WhenAttributeValueIsNull()
    {
        var attribute = new TIAttribute { Value = null };

        var result = attribute.GetValueAsString();

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetValueAsStringUpperCase_ShouldReturnNull_WhenAttributeValueIsWhitespace()
    {
        var attribute = new TIAttribute { Value = "   " };

        var result = attribute.GetValueAsStringUpperCase();

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetValueAsStringUpperCase_ShouldReturnUpperCaseTrimmedString_WhenAttributeHasValue()
    {
        var attribute = new TIAttribute { Value = "   correct value   " };

        var result = attribute.GetValueAsStringUpperCase();

        Assert.AreEqual("CORRECT VALUE", result);
    }

    [TestMethod]
    public void GetValueAsDateTime_ShouldReturnNull_WhenAttributeValueIsNull()
    {
        var attribute = new TIAttribute { Value = null };

        var result = attribute.GetValueAsDateTime();

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetValueAsDateTime_ShouldReturnNull_WhenAttributeValueIsWhitespace()
    {
        var attribute = new TIAttribute { Value = "   " };

        var result = attribute.GetValueAsDateTime();

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetValueAsDateTime_ShouldReturnDateTime_WhenAttributeHasValue()
    {
        var attribute = new TIAttribute { Value = "   2023-03-30    " };

        var result = attribute.GetValueAsDateTime();

        var expected = new DateTime(2023, 03, 30);
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void GetValueAsBool_ShouldReturnNull_WhenAttributeValueIsNull()
    {
        var attribute = new TIAttribute { Value = null };

        var result = attribute.GetValueAsString();

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetValueAsBool_ShouldReturnNull_WhenAttributeValueIsWhitespace()
    {
        var attribute = new TIAttribute { Value = "   " };

        var result = attribute.GetValueAsBool();

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetValueAsBool_ShouldReturnTrue_WhenAttributeHasTrueValue()
    {
        var attribute = new TIAttribute { Value = "   TrUe   " };

        var result = attribute.GetValueAsBool();

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void GetValueAsBool_ShouldReturnFalse_WhenAttributeHasFalseValue()
    {
        var attribute = new TIAttribute { Value = "   fAlSe   " };

        var result = attribute.GetValueAsBool();

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GetValueAsDouble_ShouldReturnNull_WhenAttributeValueIsNull()
    {
        var attribute = new TIAttribute { Value = null };

        var result = attribute.GetValueAsDouble();

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetValueAsDouble_ShouldReturnNull_WhenAttributeValueIsWhitespace()
    {
        var attribute = new TIAttribute { Value = "   " };

        var result = attribute.GetValueAsDouble();

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetValueAsDouble_ShouldReturnNull_WhenCannotParseAttributeValue()
    {
        var attribute = new TIAttribute { Value = " non-valid-number  " };

        var result = attribute.GetValueAsDouble();

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetValueAsDouble_ShouldReturnDouble_WhenAttributeHasValue()
    {
        var attribute = new TIAttribute { Value = "   123.45    " };

        var result = attribute.GetValueAsDouble();

        Assert.AreEqual(123.45d, result);
    }

    [TestMethod]
    public void HasValue_ShouldReturnFalse_WhenAttributeValueNotHasValue()
    {
        var attribute = new TIAttribute { Value = null };

        var result = attribute.HasValue();

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void HasValue_ShouldReturnFalse_WhenAttributeValueIsWhitespace()
    {
        var attribute = new TIAttribute { Value = "   " };

        var result = attribute.HasValue();

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void HasValue_ShouldReturnTrue_WhenAttributeHasValueAndIsNotBlankingSignal()
    {
        var attribute = new TIAttribute { Value = " not a blanking signal  " };

        var result = attribute.HasValue();

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void HasValue_ShouldReturnFalse_WhenAttributeHasValueAndIsBlankingSignal()
    {
        var blankingSignal = "{NULL}";
        var attribute = new TIAttribute { Value = " " + blankingSignal + "  " };

        var result = attribute.HasValue();

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void HasBlankingSignal_ShouldReturnFalse_WhenAttributeValueNotHasValue()
    {
        var attribute = new TIAttribute { Value = null };

        var result = attribute.HasBlankingSignal();

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void HasBlankingSignal_ShouldReturnFalse_WhenAttributeValueIsWhitespace()
    {
        var attribute = new TIAttribute { Value = "   " };

        var result = attribute.HasBlankingSignal();

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void HasBlankingSignal_ShouldReturnFalse_WhenAttributeHasValueAndIsNotBlankingSignal()
    {
        var attribute = new TIAttribute { Value = " not a blanking signal  " };

        var result = attribute.HasBlankingSignal();

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void HasBlankingSignal_ShouldReturnTrue_WhenAttributeHasValueAndIsBlankingSignal()
    {
        var blankingSignal = "{NULL}";
        var attribute = new TIAttribute { Value = " " + blankingSignal + "  " };

        var result = attribute.HasBlankingSignal();

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsBlankingSignal_ShouldReturnFalse_WhenAttributeHasValueAndIsNotBlankingSignal()
    {
        var attribute = new TIAttribute { Value = " not a blanking signal  " };

        var result = attribute.HasBlankingSignal();

        Assert.IsFalse(result);
    }
    [TestMethod]
    public void IsBlankingSignal_ShouldReturnFalse_WhenValueIsNotBlankingSignal()
    {
        var attribute = new TIAttribute { Value = " not a blanking signal  " };

        var result = attribute.HasBlankingSignal();

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsBlankingSignal_ShouldReturnTrue_WhenValueIsBlankingSignal()
    {
        var blankingSignal = "{NULL}";
        var attribute = new TIAttribute { Value = " " + blankingSignal + "  " };

        var result = attribute.HasBlankingSignal();

        Assert.IsTrue(result);
    }
}
