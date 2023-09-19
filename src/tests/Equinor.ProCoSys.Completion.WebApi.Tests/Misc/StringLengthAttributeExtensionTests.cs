using System.ComponentModel.DataAnnotations;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Misc;

[TestClass]
public class StringLengthAttributeExtensionTests
{
    #region test valid values
    [TestMethod]
    public void IsValid_ShouldBeTrue_ForNullValue_WithoutAnyLimitation()
    {
        // Arrange
        var attribute = new StringLengthAttribute(0);

        // Act
        var result = attribute.IsValid(null, out var message);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(message);
    }

    [TestMethod]
    public void IsValid_ShouldBeTrue_ForNullValue_WithMaxLimitationOnly()
    {
        // Arrange
        var attribute = new StringLengthAttribute(10);

        // Act
        var result = attribute.IsValid(null, out var message);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(message);
    }

    [TestMethod]
    public void IsValid_ShouldBeTrue_ForStringValue_WithoutAnyLimitation()
    {
        // Arrange
        var attribute = new StringLengthAttribute(0);

        // Act
        var result = attribute.IsValid("abc", out var message);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(message);
    }

    [TestMethod]
    public void IsValid_ShouldBeTrue_ForStringValue_WithMaxLimitationOnly()
    {
        // Arrange
        var attribute = new StringLengthAttribute(10);

        // Act
        var result = attribute.IsValid("abc", out var message);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(message);
    }

    [TestMethod]
    public void IsValid_ShouldBeTrue_ForOkStringValue_WithMinLimitationOnly()
    {
        // Arrange
        var attribute = new StringLengthAttribute(0) { MinimumLength = 1 };

        // Act
        var result = attribute.IsValid("abc", out var message);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(message);
    }
    #endregion

    #region test invalid values
    [TestMethod]
    public void IsValid_ShouldBeFalse_ForNullValue_WithMinLimitationOnly()
    {
        // Arrange
        var attribute = new StringLengthAttribute(0) { MinimumLength = 1 };

        // Act
        var result = attribute.IsValid(null, out var message);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual($"Length is 0. Length must be minimum {attribute.MinimumLength}",
            message);
    }

    [TestMethod]
    public void IsValid_ShouldBeFalse_ForNullValue_WithBothLimitations()
    {
        // Arrange
        var attribute = new StringLengthAttribute(10) { MinimumLength = 1 };

        // Act
        var result = attribute.IsValid(null, out var message);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual($"Length is 0. Length must be minimum {attribute.MinimumLength} and maximum {attribute.MaximumLength}",
            message);
    }

    [TestMethod]
    public void IsValid_ShouldBeFalse_ForToShortStringValue_WithMinLimitationOnly()
    {
        // Arrange
        var attribute = new StringLengthAttribute(0) { MinimumLength = 4 };

        // Act
        var result = attribute.IsValid("abc", out var message);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual($"Length is 3. Length must be minimum {attribute.MinimumLength}",
            message);
    }

    [TestMethod]
    public void IsValid_ShouldBeFalse_ForToShortStringValue_WithBothLimitations()
    {
        // Arrange
        var attribute = new StringLengthAttribute(10) { MinimumLength = 4 };

        // Act
        var result = attribute.IsValid("abc", out var message);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual($"Length is 3. Length must be minimum {attribute.MinimumLength} and maximum {attribute.MaximumLength}",
            message);
    }

    [TestMethod]
    public void IsValid_ShouldBeFalse_ForToLongStringValue_WithMaxLimitationOnly()
    {
        // Arrange
        var attribute = new StringLengthAttribute(2);

        // Act
        var result = attribute.IsValid("abc", out var message);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual($"Length is 3. Length must be maximum {attribute.MaximumLength}",
            message);
    }

    [TestMethod]
    public void IsValid_ShouldBeFalse_ForToLongStringValue_WithBothLimitations()
    {
        // Arrange
        var attribute = new StringLengthAttribute(2) { MinimumLength = 1 };

        // Act
        var result = attribute.IsValid("abc", out var message);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual($"Length is 3. Length must be minimum {attribute.MinimumLength} and maximum {attribute.MaximumLength}",
            message);
    }
    #endregion
}
