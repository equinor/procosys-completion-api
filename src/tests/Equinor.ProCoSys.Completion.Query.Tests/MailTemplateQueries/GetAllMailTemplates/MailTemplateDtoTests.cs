using Equinor.ProCoSys.Completion.Query.MailTemplateQueries.GetAllMailTemplates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Query.Tests.MailTemplateQueries.GetAllMailTemplates;

[TestClass]
public class MailTemplateDtoTests
{
    [TestMethod]
    public void IsGlobal_ShouldBeTrue_WhenPlantIsNull()
    {
        // Arrange
        var dut = new MailTemplateDto("c", "s", "b", false, null);

        // Act
        var result = dut.IsGlobal;

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsGlobal_ShouldBeFalse_WhenPlantHasValue()
    {
        // Arrange
        var dut = new MailTemplateDto("c", "s", "b", false, "p");

        // Act
        var result = dut.IsGlobal;

        // Assert
        Assert.IsFalse(result);
    }
}
