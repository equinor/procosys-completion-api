using Equinor.ProCoSys.Completion.TieImport.Infrastructure;

namespace Equinor.ProCoSys.Completion.TieImport.Tests;

[TestClass]
public class NumberConverterTests
{
    [TestMethod]
    public void ConvertToDecimal_ShouldReturnNull_WhenUnableToParseInput()
    {
        var result = NumberConverter.ConvertToDecimal("nonsense input");
        Assert.IsNull(result);
    }

    [TestMethod]
    public void ConvertToDecimal_ShouldReturnNumber_WhenInputHasCommaSeparator()
    {
        var result = NumberConverter.ConvertToDecimal("123,45");
        Assert.AreEqual(123.45m, result);
    }

    [TestMethod]
    public void ConvertToDecimal_ShouldReturnNumber_WhenInputHasPointSeparator()
    {
        var result = NumberConverter.ConvertToDecimal("123.45");
        Assert.AreEqual(123.45m, result);
    }
}
